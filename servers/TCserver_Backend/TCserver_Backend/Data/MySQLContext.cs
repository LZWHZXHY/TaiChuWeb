// Data/MySQLContext.cs
using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Models;

namespace TCserver_Backend.Data
{
    public class MySQLContext : DbContext
    {
        public MySQLContext(DbContextOptions<MySQLContext> options) : base(options)
        {
        }
        
        public DbSet<Financial> Financials { get; set; }

        public DbSet<useraccount> useraccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 指定实体映射到表名
            modelBuilder.Entity<Financial>().ToTable("financialtable");

            // 如果 TransactionType 是 enum，配置枚举转换为 int 存储
            modelBuilder.Entity<Financial>()
                .Property(f => f.TransactionType)
                .HasConversion<int>();

            // 建议：如果表主键不是默认的 Id，可以明确指定
            // modelBuilder.Entity<Financial>().HasKey(f => f.Id);




            modelBuilder.Entity<useraccount>(entity =>
            {
                entity.ToTable("useraccounts");

                entity.HasIndex(u => u.username).IsUnique();
                entity.HasIndex(u => u.email_address).IsUnique();

                // 设置默认值
                entity.Property(u => u.date)
                    .HasDefaultValueSql("UTC_TIMESTAMP()")
                    .ValueGeneratedOnAdd();
            });
        }
    }
}