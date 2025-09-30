using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Models.BYDlibrary;

namespace TCserver_Backend.Data
{
    public class BydSettingDbContext : DbContext
    {
        public BydSettingDbContext(DbContextOptions<BydSettingDbContext> options) : base(options)
        {
        }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<WorldBaisRules> WorldBasicRules { get; set; }
    }
}