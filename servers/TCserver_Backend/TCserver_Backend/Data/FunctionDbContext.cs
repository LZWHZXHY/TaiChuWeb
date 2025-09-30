using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Models;
using TCserver_Backend.Models.BYDlibrary;
using TCserver_Backend.Models.Game;
using TCserver_Backend.Models.Novels;
using TCserver_Backend.Models.Trade;

namespace TCserver_Backend.Data
{
    public class FunctionDbContext : DbContext
    {
        public FunctionDbContext(DbContextOptions<FunctionDbContext> options) : base(options)
        {
        }

        public DbSet<FeedBack> FeedBacks { get; set; }
        public DbSet<userdata> userdata { get; set; }

        public DbSet<useraccount> useraccount { get; set; }

        public DbSet<UserActivity> UserActivities { get; set; }

        public DbSet<Financial> Financials { get; set; }

        public DbSet<Tournaments> Tournaments { get; set; }

        public DbSet<Champions> Champions { get; set; }

        public DbSet<TournamentSignups> TournamentSignups { get; set; }

        public DbSet<ShopItem> ShopItems { get; set; }
        public DbSet<ShopOrder> ShopOrders { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; }

        public DbSet<NovelChapters> NovelChapters { get; set; }

        public DbSet<Novels> Novels { get; set; }

        public DbSet<NovelChaptersAction> NovelChaptersActions { get; set; }
    }
}
