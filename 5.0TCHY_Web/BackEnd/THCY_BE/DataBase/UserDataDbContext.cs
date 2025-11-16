using Microsoft.EntityFrameworkCore;
using THCY_BE.Models.UserDate;

namespace THCY_BE.DataBase
{
    public class UserDataDbContext : DbContext
    {
        public UserDataDbContext(DbContextOptions<UserDataDbContext> options) : base(options) { }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<UserData> UserDatas { get; set; }
    }
}
