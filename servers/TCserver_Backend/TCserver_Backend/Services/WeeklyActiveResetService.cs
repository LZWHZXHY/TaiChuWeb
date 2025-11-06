using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using TCserver_Backend.Data;

public class WeeklyActiveResetService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    public WeeklyActiveResetService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            // 计算下一个周一0点（UTC时间）
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0 && now.TimeOfDay >= TimeSpan.Zero)
                daysUntilMonday = 7; // 今天就是周一且已过0点，等下周一
            var nextMonday = now.Date.AddDays(daysUntilMonday);
            var delay = nextMonday - now;

            await Task.Delay(delay, stoppingToken);

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<FunctionDbContext>();
                db.UserActivities.RemoveRange(db.UserActivities);
                await db.SaveChangesAsync();
                // 建议写日志方便监控
                // _logger.LogInformation("已清空活跃度表：" + DateTime.UtcNow);
            }
        }
    }
}