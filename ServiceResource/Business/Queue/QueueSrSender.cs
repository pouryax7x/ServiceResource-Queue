using Azure.Core;
using Newtonsoft.Json;
using Quartz;
using RabbitMQ.Client;
using ServiceResource.Dto;
using ServiceResource.Enums;
using ServiceResource.Interfaces;
using ServiceResource.Persistence.Queue.Entities;
using System.Text;
using YouRest.Interface;
using static Quartz.Logging.OperationName;

namespace ServiceResource.Business.Queue
{
    public class QueueSrSender : QueueReciverBase
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private static int currentCallCount = 0;
        public QueueSrSender(ISR_Service sR_Service, IQueueRepository queueRepository, IQueueHandler queueHandler)
        {
            SR_Service = sR_Service;
            QueueRepository = queueRepository;
            QueueHandler = queueHandler;
        }

        public ISR_Service SR_Service { get; }
        public IQueueRepository QueueRepository { get; }
        public IQueueHandler QueueHandler { get; }

        public override async Task Execute(IJobExecutionContext context)
        {
            await semaphore.WaitAsync();
            try
            {
                var methodName = (MethodName)Enum.Parse(typeof(MethodName), context.MergedJobDataMap["MethodName"].ToString());
                var QSetting = await QueueRepository.GetQueueSetting(methodName);

                var factory = QueueRepository.GetFactory();
                var channel = factory.CreateConnection().CreateModel();
                var arguments = new Dictionary<string, object>
                    {
                        { "x-message-deduplication", true } // Enable message deduplication for the queue
                    };
                channel.QueueDeclare(queue: QSetting.MethodName.ToString(), durable: true, exclusive: false, autoDelete: false, arguments: arguments);

                for (int i = 0; i < QSetting.MaxCallsPerInterval; i++)
                {
                    if (currentCallCount < QSetting.MaxCallsPerInterval)
                    {
                        Task.Run(() =>
                        {
                            StartConsumer(channel, QSetting);
                        });
                        currentCallCount++;
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task StartConsumer(IModel channel, QueueSetting qSetting)
        {
            try
            {
                var rawmessage = channel.BasicGet(qSetting.MethodName.ToString(), false);
                if (rawmessage == null)
                {
                    return;
                }

                var body = rawmessage.Body.ToArray();
                var callCount = (int)rawmessage.BasicProperties.Headers["CallCount"];
                if (callCount >= qSetting.MaxCallCount && qSetting.MaxCallCount != -1)
                {
                    channel.BasicAck(rawmessage.DeliveryTag, multiple: false);
                    return;
                }

                var message = Encoding.UTF8.GetString(body);
                var requestBody = JsonConvert.DeserializeObject<SRRequest>(message);
                requestBody.CallingMode = Enums.ServiceCallingMode.ImmediateWithCheckResult;
                var result = await SR_Service.CallProcessAsync(requestBody);
                if (result.Success == Enums.SuccessInfo.Success)
                {
                    channel.BasicAck(rawmessage.DeliveryTag, multiple: false);
                    await QueueHandler.InsertInQueueCallBack(result.Response, qSetting.MethodName, 0);
                }
                else
                {
                    await BackToEndOfTheQueue(channel, rawmessage, requestBody, callCount);
                }

            }
            finally
            {
                Interlocked.Decrement(ref currentCallCount);
                await Task.FromResult(true);
            }

        }

        private static void BackToFrontOfTheQueue(IModel channel, BasicGetResult rawmessage)
        {
            channel.BasicNack(rawmessage.DeliveryTag, multiple: false, requeue: true);
        }

        private async Task BackToEndOfTheQueue(IModel channel, BasicGetResult rawmessage, SRRequest request, int callCount)
        {
            channel.BasicNack(rawmessage.DeliveryTag, multiple: false, requeue: false);
            await QueueHandler.InsertInQueue(request, callCount + 1);
        }
    }
}
