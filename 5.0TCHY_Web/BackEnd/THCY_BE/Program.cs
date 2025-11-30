using Microsoft.EntityFrameworkCore;
using System.Reflection;
using THCY_BE.DataBase;
using THCY_BE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Hosting; // 添加这个命名空间

var builder = WebApplication.CreateBuilder(args);

// 1) 只用一个连接串：DefaultConnection（你的腾讯云 CynosDB MySQL）
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");

// 建议：为云数据库加上超时配置，避免卡住（可选）
// 也可以直接在 appsettings.json 的连接串后追加：";Connection Timeout=5;Default Command Timeout=8;"
var serverVersion = new MySqlServerVersion(new Version(8, 0, 30));

// 2) 注册你项目所有用到的 DbContext，全部使用同一个 DefaultConnection
builder.Services.AddDbContext<BasicInfoDbContext>(options =>
    options.UseMySql(defaultConn, serverVersion));

builder.Services.AddDbContext<ChaiDbContext>(options =>
    options.UseMySql(defaultConn, serverVersion));

builder.Services.AddDbContext<UserDataDbContext>(options =>
    options.UseMySql(defaultConn, serverVersion));

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseMySql(defaultConn, serverVersion));

builder.Services.AddDbContext<PostsDbContext>(options =>
    options.UseMySql(defaultConn, serverVersion));

builder.Services.AddDbContext<FriendsDbContext>(options =>
    options.UseMySql(defaultConn, serverVersion));

builder.Services.AddDbContext<FeedBackDbContext>(options =>
    options.UseMySql(defaultConn, serverVersion));


// 注册内存缓存（B 站封面缓存会用到）
builder.Services.AddMemoryCache();

// 注册 HttpClientFactory（FileUploadService 需要）
builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ImageUrlService>();

// 注册 FileUploadService - 确保注入 IWebHostEnvironment


// 注册命名 HttpClient，用于调用 B 站 API（后端代理）
// 超时、User-Agent 可在此统一设置
builder.Services.AddHttpClient("bili-client", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("THCY_BE/1.0 (+https://yourdomain.example)");
    client.Timeout = TimeSpan.FromSeconds(8);
});

// 添加控制器和API支持
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();








// Swagger：添加 JWT 授权支持，方便开发时在 Swagger 中呼叫受保护接口
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "THCY_BE API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };
    c.AddSecurityDefinition("bearerAuth", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            Array.Empty<string>()
        }
    });
});

// 直接从配置中读取JWT设置
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (!string.IsNullOrEmpty(jwtSecretKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
                ValidIssuer = jwtIssuer,
                ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
}

builder.Services.AddAuthorization();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVerificationCodeService, VerificationCodeService>();

// 配置CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://bianyuzhou.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // 保持允许凭据
    });
});

var app = builder.Build();

// 启动时显示环境信息
app.Logger.LogInformation("=== 应用启动信息 ===");
app.Logger.LogInformation("环境: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("根路径: {ContentRootPath}", app.Environment.ContentRootPath);
app.Logger.LogInformation("Web根路径: {WebRootPath}", app.Environment.WebRootPath);
app.Logger.LogInformation("NAS端点: {NasEndpoint}", app.Configuration["StorageSettings:NasEndpoint"]);
app.Logger.LogInformation("是否开发环境: {IsDevelopment}", app.Environment.IsDevelopment());
app.Logger.LogInformation("是否生产环境: {IsProduction}", app.Environment.IsProduction());

// 开发环境配置
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "THCY_BE API v1");
        c.RoutePrefix = "swagger"; // 将Swagger设置为根路径
    });

    // 开发环境启用详细错误页面
    app.UseDeveloperExceptionPage();
}
else
{
    // 生产环境使用更简洁的错误处理
    app.UseExceptionHandler("/error");
    app.UseHsts(); // 添加HSTS安全头
}

app.UseHttpsRedirection();

// 启用静态文件服务（FileUploadService 需要）
app.UseStaticFiles();

// CORS 必须在 UseAuthentication/UseAuthorization 之前或至少在路由/端点处理前调用
app.UseCors("AllowVueApp");

app.UseAuthentication();
app.UseAuthorization();

// 添加健康检查端点
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    nasEndpoint = app.Configuration["StorageSettings:NasEndpoint"] ?? "Not configured"
}));

// 添加环境信息端点
app.MapGet("/api/environment", () => new
{
    Environment = app.Environment.EnvironmentName,
    IsDevelopment = app.Environment.IsDevelopment(),
    IsProduction = app.Environment.IsProduction(),
    MachineName = Environment.MachineName,
    NasEndpoint = app.Configuration["StorageSettings:NasEndpoint"],
    ApplicationName = app.Environment.ApplicationName,
    ContentRootPath = app.Environment.ContentRootPath,
    WebRootPath = app.Environment.WebRootPath
});

// 添加错误处理端点
app.MapGet("/error", () => Results.Problem("服务器内部错误"));

// 控制器映射
try
{
    app.MapControllers();
    app.Logger.LogInformation("✅ 控制器映射成功");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "❌ 控制器映射失败");

    // 提供友好的错误页面
    app.Map("/error", () => "应用程序启动失败，请检查控制台日志");

    // 在开发环境下显示详细错误
    if (app.Environment.IsDevelopment())
    {
        app.Map("/debug", () => $"错误详情: {ex.Message}\n\n堆栈跟踪: {ex.StackTrace}");
    }
}

// 添加全局异常处理中间件
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        app.Logger.LogError(ex, "全局异常处理: 未处理的异常");

        if (app.Environment.IsDevelopment())
        {
            await context.Response.WriteAsJsonAsync(new
            {
                error = "内部服务器错误",
                message = ex.Message,
                stackTrace = ex.StackTrace,
                environment = app.Environment.EnvironmentName
            });
        }
        else
        {
            await context.Response.WriteAsJsonAsync(new
            {
                error = "内部服务器错误",
                message = "请稍后重试"
            });
        }
    }
});

app.Run();