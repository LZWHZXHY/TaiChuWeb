using Microsoft.EntityFrameworkCore;
using THCY_BE.Models.Chai;
namespace THCY_BE.DataBase
{
    public class ChaiDbContext:DbContext
    {
        public ChaiDbContext(DbContextOptions<ChaiDbContext> options) : base(options) { }

        public DbSet<Models.Chai.ChaiSheTuan> ChaiSheTuans { get; set; }

        public DbSet<ChaiLianHe> chaiLianHes { get; set; }

        public DbSet<VideoRecommend> videoRecommends { get; set; }
        
        public DbSet<OC_Info> OC_Infos { get; set; }
    }
}
