using Quartz;
using RabbitMQ.Client;
using System.Threading.Channels;

namespace ServiceResource.Business.Queue
{
    [DisallowConcurrentExecution]
    public abstract class QueueReciverBase : IJob
    {
        public abstract Task Execute(IJobExecutionContext context);
    }
}
