using Microsoft.EntityFrameworkCore;
using THCY_BE.Models.UserDate;

namespace THCY_BE.DataBase
{
    public class PostsDbContext : DbContext
    {
        public PostsDbContext(DbContextOptions<PostsDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Posts.Post> Posts { get; set; }
        public DbSet<Models.Posts.Comment> Comments { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<UserData> UserDatas { get; set; }
    }
}
