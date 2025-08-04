using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TCserver_Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// ==== 日志（开发阶段打开详细输出） ====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ==== 读取连接串 ====
var connectionString = builder.Configuration.GetConnectionString("MySQL");

// ==== 这里要用实际 MySQL 版本（先用 SELECT VERSION() 查） ====
var serverVersion = new MySqlServerVersion(new Version(8, 0, 30));
// 举例，改成你数据库实际版本

// ==== 添加数据库上下文：重试 + 详细错误 ====
builder.Services.AddDbContext<MySQLContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySQL"),
        new MySqlServerVersion(new Version(8, 0, 30)), // ← 用你查到的版本
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });

    options.EnableSensitiveDataLogging(); // 开发调试阶段保留，生产去掉
    options.EnableDetailedErrors();       // 开发调试阶段保留
});


// ==== 控制器/页面/Swagger ====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

builder.Services.AddRazorPages();

// ==== CORS（开发阶段允许所有） ====
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
    // app.UseSwagger();
    // app.UseSwaggerUI();
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
