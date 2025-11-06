using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Models;

namespace TCserver_Backend.Data
{
    public class ChailianheDbContext:DbContext
    {
        public ChailianheDbContext(DbContextOptions<ChailianheDbContext> options) : base(options)
        {
        }
        public DbSet<chailianhe> Chailianhes { get; set; }
    }
}
