using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Models;

namespace TCserver_Backend.Data
{
    public class RegisterContext : DbContext
    {
        public RegisterContext(DbContextOptions<RegisterContext> options) : base(options)
        {
        }
        public DbSet<PostsLike> PostsLike { get; set; }
        public DbSet<useraccount> UserAccounts { get; set; }
        public DbSet<userdata> UserDatas { get; set; }
        public DbSet<Posts> Posts { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Comments> Comments { get; set; }

        public DbSet<PostReport> PostReports { get; set; }
        public DbSet<CommentReport> CommentReports { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostsLike>()
                .HasIndex(l => new { l.PostId, l.UserId })
                .IsUnique()
                .HasDatabaseName("uniq_post_user");



            modelBuilder.Entity<EmailVerification>().ToTable("emailverification");

            modelBuilder.Entity<useraccount>()
                .HasOne(u => u.userdata)
                .WithOne(u => u.useraccount)
                .HasForeignKey<userdata>(u => u.id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<userdata>()
                .Property(u => u.points)
                .HasDefaultValue(0);

            modelBuilder.Entity<userdata>()
                .Property(u => u.level)
                .HasDefaultValue(0);

            // 配置Posts实体关系
            modelBuilder.Entity<Posts>(entity =>
            {
                entity.ToTable("posts");
                entity.HasKey(p => p.id);

                entity.HasOne(p => p.useraccount)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(p => p.author_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}