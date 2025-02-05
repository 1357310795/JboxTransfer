using JboxTransfer.Core.Models.Db;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace JboxTransfer.Core.Modules.Db
{
    public class DefaultDbContext : DbContext
    {
        public DbSet<SystemUser> Users { set; get; }
        public DbSet<SyncTaskDbModel> SyncTasks { set; get; }

        public DefaultDbContext(DbContextOptions<DefaultDbContext> Options) : base(Options)
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
        }
    }
}
