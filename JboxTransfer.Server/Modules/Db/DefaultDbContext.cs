using JboxTransfer.Server.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace JboxTransfer.Server.Modules.Db
{
    public class DefaultDbContext : DbContext
    {
        public DbSet<SystemUser> Users { set; get; }
        public DefaultDbContext(DbContextOptions<DefaultDbContext> Options) : base(Options)
        {

        }
    }
}
