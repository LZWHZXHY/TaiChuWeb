using Microsoft.EntityFrameworkCore;

namespace THCY_BE.DataBase
{
    public class DrawingDbContext : DbContext
    {
        public DrawingDbContext(DbContextOptions<DrawingDbContext> options) : base(options)
        {
        }

        public DbSet<THCY_BE.Models.Draws.Drawing> Drawings { get; set; }

        
    }
}
