using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TCserver_Backend.Data;
using TCserver_Backend.Models;
using TCserver_Backend.Services;


var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment;

// ==== 日志（开发阶段打开详细输出） ====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
//builder.Logging.AddDebug(); // 用于Visual Studio输出窗口
builder.Logging.SetMinimumLevel(LogLevel.Trace);



// 添加配置
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();










// ==== 读取连接串 ====
//var financeConnStr = builder.Configuration.GetConnectionString("MySQL");
var registerConnStr = builder.Configuration.GetConnectionString("MySQL_Register");
var applicationConnStr = builder.Configuration.GetConnectionString("Application");
var chailianheConnStr = builder.Configuration.GetConnectionString("Chailianhe");
var checkInConnStr = builder.Configuration.GetConnectionString("CheckIn");
var postsConnStr = builder.Configuration.GetConnectionString("Posts");
var functionStr = builder.Configuration.GetConnectionString("Function");
var bydLibraryStr = builder.Configuration.GetConnectionString("BYDlibrary");

// ==== 使用实际 MySQL 版本 ====
var serverVersion = new MySqlServerVersion(new Version(8, 0, 30));







builder.Services.AddDbContext<BydSettingDbContext>(options =>
    options.UseMySql(bydLibraryStr, serverVersion)
);






//添加CheckIn数据库上下文
builder.Services.AddDbContext<CheckInDateDbContext>(options =>
    options.UseMySql(checkInConnStr, serverVersion)
);

//添加Posts数据库上下文
builder.Services.AddDbContext<PostsDbContext>(options =>
    options.UseMySql(postsConnStr, serverVersion)
);

//添加Application数据库上下文
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(applicationConnStr, serverVersion)
);

builder.Services.AddDbContext<FunctionDbContext>(options =>
    options.UseMySql(functionStr, serverVersion)
);

// 添加Chailianhe数据库上下文
builder.Services.AddDbContext<ChailianheDbContext>(options =>
    options.UseMySql(chailianheConnStr, serverVersion)
);

// 配置数据库上下文
builder.Services.AddDbContext<RegisterContext>(options =>
{
    options.UseMySql(
        registerConnStr,
        serverVersion,
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });
});


// ======= 密码加密器依赖注入 =========
builder.Services.AddScoped<IPasswordHasher<useraccount>, PasswordHasher<useraccount>>();

// ======= 邮件发送服务 =========
builder.Services.AddScoped<EmailSender>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<EmailSender>>();
    var env = sp.GetRequiredService<IHostEnvironment>();
    return new EmailSender(config, logger, env);
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Environment.IsDevelopment()
                ? builder.Configuration["Jwt:Issuer:Development"]
                : builder.Configuration["Jwt:Issuer:Production"],
            ValidAudience = builder.Environment.IsDevelopment()
                ? builder.Configuration["Jwt:Audience:Development"]
                : builder.Configuration["Jwt:Audience:Production"],
            ClockSkew = TimeSpan.Zero
        };
    });






// ==== 添加自适应NAS服务 ====
builder.Services.AddHttpClient("NasClient", client =>
{
    var nasEndpoint = builder.Configuration["StorageSettings:NasEndpoint"];
    var nasUsername = builder.Configuration["NasCredentials:Username"];
    var nasPassword = builder.Configuration["NasCredentials:Password"];

    client.BaseAddress = new Uri(nasEndpoint);
    client.Timeout = TimeSpan.FromSeconds(10);

    // 设置基本认证
    var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{nasUsername}:{nasPassword}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
});











builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});













// ==== 控制器/页面 ====
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        opts.JsonSerializerOptions.WriteIndented = false;
    });

builder.Services.AddMemoryCache();


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddRazorPages();

builder.Services.AddHttpClient();
builder.Services.AddHostedService<WeeklyActiveResetService>();

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<RefreshTokenService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ==== 异常页/环境区分 ====
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();



app.MapControllers();
app.MapRazorPages();


app.Run();