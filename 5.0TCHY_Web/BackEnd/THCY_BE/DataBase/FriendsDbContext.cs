using Microsoft.EntityFrameworkCore;
using THCY_BE.Models.Friends;
using THCY_BE.Models.UserDate;

namespace THCY_BE.DataBase
{
    public class FriendsDbContext : DbContext
    {
        public FriendsDbContext(DbContextOptions<FriendsDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Friends.FriendRequest> FriendRequests { get; set; }
        public DbSet<Models.Friends.Friend> Friends { get; set; }

        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<UserData> UserDatas { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 好友关系唯一约束
            modelBuilder.Entity<Friend>()
                .HasIndex(f => new { f.UserId, f.FriendId })
                .IsUnique();

            // 好友申请唯一约束（避免重复申请）
            modelBuilder.Entity<FriendRequest>()
                .HasIndex(fr => new { fr.FromUserId, fr.ToUserId, fr.Status })
                .IsUnique()
                .HasFilter("[Status] = 0"); // 只对未处理的申请做唯一约束

            // 用户账户配置
            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(u => u.userdata)
                      .WithOne(ud => ud.useraccount)
                      .HasForeignKey<UserData>(ud => ud.id);
            });
        }
    }
}
