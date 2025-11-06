// Data/PostsDbContext.cs
using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Models;

namespace TCserver_Backend.Data
{
    public class PostsDbContext : DbContext
    {
        public PostsDbContext(DbContextOptions<PostsDbContext> options) : base(options)
        {
        }

        public DbSet<Posts> Posts { get; set; }

        public DbSet<Comments> Comments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 配置Posts实体
            modelBuilder.Entity<Posts>(entity =>
            {
                entity.ToTable("posts");
                entity.HasKey(p => p.id);

                // 配置关系：一个用户可以有多个帖子
                entity.HasOne(p => p.useraccount)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(p => p.author_id)
                    .OnDelete(DeleteBehavior.Restrict); // 根据需求设置删除行为
            });
        }
    }
}