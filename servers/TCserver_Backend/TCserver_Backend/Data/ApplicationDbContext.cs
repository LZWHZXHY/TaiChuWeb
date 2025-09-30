using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Models;

namespace TCserver_Backend.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<shetuan> Shetuans { get; set; }

        

        
    }
}
