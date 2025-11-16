using Microsoft.EntityFrameworkCore;
using System.Reflection;
using THCY_BE.DataBase;
using THCY_BE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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


// 添加控制器和API支持
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://bianyuzhou.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // 添加这一行
    });
});

var app = builder.Build();

// 开发环境配置
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowVueApp");
app.UseAuthentication();
app.UseAuthorization();

// 添加错误处理
try
{
    app.MapControllers();
    Console.WriteLine("控制器映射成功");
}
catch (ReflectionTypeLoadException ex)
{
    Console.WriteLine("控制器映射失败，详细错误:");
    foreach (var loaderException in ex.LoaderExceptions)
    {
        Console.WriteLine($"{loaderException.Message}");
        if (loaderException.InnerException != null)
        {
            Console.WriteLine($"内部错误: {loaderException.InnerException.Message}");
        }
    }

    // 提供友好的错误页面
    app.Map("/error", () => "应用程序启动失败，请检查控制台日志");
}







app.Run();