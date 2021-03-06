using Microsoft.EntityFrameworkCore;
using Taledynamic.DAL.Entities;

namespace Taledynamic.Core
{
    public class TaledynamicContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public TaledynamicContext(DbContextOptions<TaledynamicContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Workspace>()
                .HasIndex(p => p.UserId);
            modelBuilder.Entity<Table>()
                .HasIndex(p => p.WorkspaceId);
            modelBuilder.Entity<TelegramUser>()
                .HasIndex(p => p.UserId);
        }
    }
}