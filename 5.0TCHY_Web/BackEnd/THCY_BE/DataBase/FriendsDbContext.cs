using Microsoft.EntityFrameworkCore;
using THCY_BE.Models.UserDate;

namespace THCY_BE.DataBase
{
    public class FriendsDbContext : DbContext
    {
        public FriendsDbContext(DbContextOptions<FriendsDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Friends.FriendRequest> FriendRequests { get; set; }
        public DbSet<Models.Friends.Friends> Friends { get; set; }

        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<UserData> UserDatas { get; set; }
    }
}
