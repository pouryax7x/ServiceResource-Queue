using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using ServiceResource.Dto;
using ServiceResource.Enums;
using ServiceResource.Interfaces;
using ServiceResource.Persistence.Queue.Context;
using ServiceResource.Persistence.Queue.Entities;

namespace ServiceResource.Business.Queue
{
    public class QueueRepository : IQueueRepository
    {
        public static List<QueueSetting> QueueSettings { get; set; }
        public QueueRepository(IConfiguration configuration)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QueueContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("Queue_DB_Address"));

            using (QueueContext dbContext = new QueueContext(optionsBuilder.Options))
            {
                if (QueueSettings == null || QueueSettings.Count == 0)
                {
                    QueueSettings = dbContext.QueueSetting.ToList();
                }
            }
        }

        public async Task<QueueSetting> GetQueueSetting(MethodName methodName)
        {
            return QueueSettings.Where(x => x.MethodName == methodName).FirstOrDefault() ?? throw new Exception();
        }

        public async Task<List<QueueSetting>> GetQueueSettings()
        {
            return QueueSettings;
        }

        public ConnectionFactory GetFactory()
        {
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = "localhost", // Replace with your RabbitMQ server hostname
                UserName = "guest",     // Replace with your RabbitMQ username
                Password = "guest",     // Replace with your RabbitMQ password
            };
            return factory;
        }
    }
}
