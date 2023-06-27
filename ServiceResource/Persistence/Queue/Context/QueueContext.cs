using Microsoft.EntityFrameworkCore;
using ServiceResource.Interfaces;
using ServiceResource.Persistence.Queue.Entities;

namespace ServiceResource.Persistence.Queue.Context
{
    public class QueueContext : DbContext, IQueueContext
    {
        public QueueContext(DbContextOptions<QueueContext> options) : base(options)
        {

        }
        public DbSet<QueueSetting> QueueSetting { get; set; }
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
