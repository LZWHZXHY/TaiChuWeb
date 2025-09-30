using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Models;

namespace TCserver_Backend.Data
{
    public class CheckInDateDbContext: DbContext
    {
        public CheckInDateDbContext(DbContextOptions<CheckInDateDbContext> options) : base(options)
        {
        }
        public DbSet<UserCheckinRecord> UserCheckinRecords { get; set; }

        public DbSet<useraccount> UserAccounts { get; set; }

        public DbSet<userdata> UserDatas { get; set; }
    }
}
