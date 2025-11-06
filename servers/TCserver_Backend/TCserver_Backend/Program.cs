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

// ==== ��־�������׶δ���ϸ����� ====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
//builder.Logging.AddDebug(); // ����Visual Studio�������
builder.Logging.SetMinimumLevel(LogLevel.Trace);



// ��������
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();










// ==== ��ȡ���Ӵ� ====
//var financeConnStr = builder.Configuration.GetConnectionString("MySQL");
var registerConnStr = builder.Configuration.GetConnectionString("MySQL_Register");
var applicationConnStr = builder.Configuration.GetConnectionString("Application");
var chailianheConnStr = builder.Configuration.GetConnectionString("Chailianhe");
var checkInConnStr = builder.Configuration.GetConnectionString("CheckIn");
var postsConnStr = builder.Configuration.GetConnectionString("Posts");
var functionStr = builder.Configuration.GetConnectionString("Function");
var bydLibraryStr = builder.Configuration.GetConnectionString("BYDlibrary");

// ==== ʹ��ʵ�� MySQL �汾 ====
var serverVersion = new MySqlServerVersion(new Version(8, 0, 30));







builder.Services.AddDbContext<BydSettingDbContext>(options =>
    options.UseMySql(bydLibraryStr, serverVersion)
);






//����CheckIn���ݿ�������
builder.Services.AddDbContext<CheckInDateDbContext>(options =>
    options.UseMySql(checkInConnStr, serverVersion)
);

//����Posts���ݿ�������
builder.Services.AddDbContext<PostsDbContext>(options =>
    options.UseMySql(postsConnStr, serverVersion)
);

//����Application���ݿ�������
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(applicationConnStr, serverVersion)
);

builder.Services.AddDbContext<FunctionDbContext>(options =>
    options.UseMySql(functionStr, serverVersion)
);

// ����Chailianhe���ݿ�������
builder.Services.AddDbContext<ChailianheDbContext>(options =>
    options.UseMySql(chailianheConnStr, serverVersion)
);

// �������ݿ�������
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


// ======= �������������ע�� =========
builder.Services.AddScoped<IPasswordHasher<useraccount>, PasswordHasher<useraccount>>();

// ======= �ʼ����ͷ��� =========
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






// ==== ��������ӦNAS���� ====
builder.Services.AddHttpClient("NasClient", client =>
{
    var nasEndpoint = builder.Configuration["StorageSettings:NasEndpoint"];
    var nasUsername = builder.Configuration["NasCredentials:Username"];
    var nasPassword = builder.Configuration["NasCredentials:Password"];

    client.BaseAddress = new Uri(nasEndpoint);
    client.Timeout = TimeSpan.FromSeconds(10);

    // ���û�����֤
    var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{nasUsername}:{nasPassword}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
});











builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});













// ==== ������/ҳ�� ====
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

// ==== �쳣ҳ/�������� ====
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