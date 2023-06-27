using Microsoft.EntityFrameworkCore;
using ServiceResource.Persistence.Queue.Entities;

namespace ServiceResource.Interfaces
{
    public interface IQueueContext
    {
        public DbSet<QueueSetting> QueueSetting { get; set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
