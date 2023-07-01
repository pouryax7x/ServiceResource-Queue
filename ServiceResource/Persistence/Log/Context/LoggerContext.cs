using Microsoft.EntityFrameworkCore;
using ServiceResource.Interfaces;
using ServiceResource.Persistence.Log.Entities;
using ServiceResource.Persistence.Queue.Context;

namespace ServiceResource.Persistence.Log.Context
{
    public class LogContext : DbContext, ILogContext
    {
        public LogContext(DbContextOptions<LogContext> options) : base(options)
        {

        }
        public DbSet<RequestLog> RequestLog { get; set; }
        public DbSet<ResponseLog> ResponseLog { get; set; }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            int count = await base.SaveChangesAsync(cancellationToken);
            this.ChangeTracker.Clear();
            return count;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                try
                {
                }
                catch (Exception)
                {
                    // throw new Exception("Database Config has a problem");
                }
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
