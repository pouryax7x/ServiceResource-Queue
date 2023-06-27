using ServiceResource.Persistence.Log.Entities;

namespace ServiceResource.Interfaces
{
    public interface ILoggerContext
    {
        public bool Log(RequestLog log);
        public bool Log(ResponseLog log);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
