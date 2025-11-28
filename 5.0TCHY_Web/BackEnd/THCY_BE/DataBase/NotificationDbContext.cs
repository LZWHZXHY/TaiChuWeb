using Microsoft.EntityFrameworkCore;
using THCY_BE.Models.Notification;
using THCY_BE.Models.UserDate;

namespace THCY_BE.DataBase
{
    public class NotificationDbContext:DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        { 

        }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<UserData> UserDatas { get; set; }

    }
}
