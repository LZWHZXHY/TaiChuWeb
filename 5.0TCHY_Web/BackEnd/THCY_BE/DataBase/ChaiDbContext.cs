// DataBase/ChaiDbContext.cs
using Microsoft.EntityFrameworkCore;
using THCY_BE.Models.Chai;

namespace THCY_BE.DataBase
{
    public class ChaiDbContext : DbContext
    {
        public ChaiDbContext(DbContextOptions<ChaiDbContext> options) : base(options) { }

        public DbSet<Models.Chai.ChaiSheTuan> ChaiSheTuans { get; set; }
        public DbSet<ChaiLianHe> chaiLianHes { get; set; }
        public DbSet<VideoRecommend> videoRecommends { get; set; }

        // 修正DbSet名称
        public DbSet<OC_Versions> OC_Versions { get; set; }
        public DbSet<OC_Master> OC_Master { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置OC_Master和OC_Versions的关系
            modelBuilder.Entity<OC_Versions>()
                .HasOne(v => v.OCMaster)
                .WithMany(m => m.Versions)
                .HasForeignKey(v => v.ocMasterId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置表名和主键
            modelBuilder.Entity<OC_Master>(entity =>
            {
                entity.ToTable("oc_master");
                entity.HasKey(e => e.id);
                entity.Property(e => e.id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<OC_Versions>(entity =>
            {
                entity.ToTable("oc_versions");
                entity.HasKey(e => e.id);
                entity.Property(e => e.id).ValueGeneratedOnAdd();
            });
        }
    }
}