using Microsoft.EntityFrameworkCore;
namespace THCY_BE.DataBase
{
    public class ChaiDbContext:DbContext
    {
        public ChaiDbContext(DbContextOptions<ChaiDbContext> options) : base(options) { }

        public DbSet<Models.Chai.ChaiSheTuan> ChaiSheTuans { get; set; }
    }
}
