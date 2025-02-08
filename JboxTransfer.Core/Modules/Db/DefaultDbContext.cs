using JboxTransfer.Core.Models.Db;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Modules.Db
{
    public class DefaultDbContext : DbContext
    {
        public DbSet<SystemUser> Users { set; get; }
        public DbSet<UserStatistics> UserStats { set; get; }
        public DbSet<SyncTaskDbModel> SyncTasks { set; get; }

        public object insertLock = new object();

        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<SyncTaskDbModel>()
                .Property(e => e.Type)
                .HasConversion<string>();            
            modelBuilder
                .Entity<SyncTaskDbModel>()
                .Property(e => e.State)
                .HasConversion<string>();
            modelBuilder.Entity<SystemUser>()
                .HasOne(e => e.Stat)
                .WithMany()
                .HasForeignKey(e => e.StatId)
                .IsRequired();
        }

        public int GetMinOrder()
        {
            var res = SyncTasks.OrderBy(x => x.Order).FirstOrDefault();
            if (res == null)
                return 0;
            else
                return res.Order;
        }

        public int GetMaxOrder()
        {
            var res = SyncTasks.OrderByDescending(x => x.Order).FirstOrDefault();
            if (res == null)
                return 0;
            else
                return res.Order;
        }
    }
}
