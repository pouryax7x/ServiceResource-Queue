using Microsoft.EntityFrameworkCore;
using ServiceResource.Persistence.Log.Entities;
using ServiceResource.Persistence.Queue.Entities;

namespace ServiceResource.Interfaces
{
    public interface ILogContext
    {
        public DbSet<RequestLog> RequestLog { get; set; }
        public DbSet<ResponseLog> ResponseLog { get; set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
