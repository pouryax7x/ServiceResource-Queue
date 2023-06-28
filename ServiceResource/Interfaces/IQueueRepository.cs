using RabbitMQ.Client;
using ServiceResource.Dto;
using ServiceResource.Enums;
using ServiceResource.Persistence.Queue.Entities;

namespace ServiceResource.Interfaces
{
    public interface IQueueRepository
    {
        public ConnectionFactory GetFactory();
        public Task<QueueSetting> GetQueueSetting(MethodName methodName);
        public Task<List<QueueSetting>> GetQueueSettings();
    }
}
