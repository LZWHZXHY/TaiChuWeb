using Microsoft.EntityFrameworkCore;
using System.Reflection;
using THCY_BE.DataBase;

var builder = WebApplication.CreateBuilder(args);

// 添加数据库上下文
builder.Services.AddDbContext<BasicInfoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 30))
    ));

// 添加控制器和API支持
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://bianyuzhou.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
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