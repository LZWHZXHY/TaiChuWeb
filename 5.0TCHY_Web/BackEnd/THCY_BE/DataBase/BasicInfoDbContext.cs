using Microsoft.EntityFrameworkCore;
using THCY_BE.Models.LoginRegister;
using THCY_BE.Models.UserDate;


namespace THCY_BE.DataBase
{
    public class BasicInfoDbContext : DbContext
    {
        public BasicInfoDbContext(DbContextOptions<BasicInfoDbContext> options) : base(options) { }

        // 用户相关表
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<UserData> UserDatas { get; set; }

        public DbSet<EmailVerification> EmailVerifications { get; set; }


    }
}
