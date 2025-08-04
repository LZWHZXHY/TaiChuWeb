using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TCserver_Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// ==== ��־�������׶δ���ϸ����� ====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ==== ��ȡ���Ӵ� ====
var connectionString = builder.Configuration.GetConnectionString("MySQL");

// ==== ����Ҫ��ʵ�� MySQL �汾������ SELECT VERSION() �飩 ====
var serverVersion = new MySqlServerVersion(new Version(8, 0, 30));
// �������ĳ������ݿ�ʵ�ʰ汾

// ==== ������ݿ������ģ����� + ��ϸ���� ====
builder.Services.AddDbContext<MySQLContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySQL"),
        new MySqlServerVersion(new Version(8, 0, 30)), // �� ����鵽�İ汾
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });

    options.EnableSensitiveDataLogging(); // �������Խ׶α���������ȥ��
    options.EnableDetailedErrors();       // �������Խ׶α���
});


// ==== ������/ҳ��/Swagger ====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

builder.Services.AddRazorPages();

// ==== CORS�������׶��������У� ====
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
