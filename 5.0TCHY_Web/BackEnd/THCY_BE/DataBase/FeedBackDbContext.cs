using Microsoft.EntityFrameworkCore;

namespace THCY_BE.DataBase
{
    public class FeedBackDbContext : DbContext
    {
        public FeedBackDbContext(DbContextOptions<FeedBackDbContext> options) : base(options)
        {
        }
        public DbSet<Models.FeedBack.Feedback> FeedBacks { get; set; }
    }
}
