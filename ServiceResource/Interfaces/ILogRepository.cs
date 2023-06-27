using ServiceResource.Persistence.Log.Entities;

namespace ServiceResource.Interfaces
{
    public interface ILogRepository
    {
        public Task Log(RequestLog requestLog);
        public Task Log(ResponseLog responseLog);
    }
}
