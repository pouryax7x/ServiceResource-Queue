using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Quartz;
using RabbitMQ.Client;
using ServiceResource.Dto;
using ServiceResource.Enums;
using ServiceResource.Infrastructure;
using ServiceResource.Interfaces;
using ServiceResource.Persistence.Log.Entities;
using ServiceResource.Persistence.Queue.Entities;
using System.Reflection;
using System.Text;
using YouRest;
using YouRest.Interface.Body;

namespace ServiceResource.Business.Queue
{
    public class QueueCallbackSender : QueueReciverBase
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private static int currentCallCount = 0;
        public QueueCallbackSender(ISR_Service sR_Service,
                                   IQueueRepository queueRepository,
                                   IQueueHandler queueHandler,
                                   ILogRepository logRepository)
        {
            SR_Service = sR_Service;
            QueueRepository = queueRepository;
            QueueHandler = queueHandler;
            LogRepository = logRepository;
        }

        public ISR_Service SR_Service { get; }
        public IQueueRepository QueueRepository { get; }
        public IQueueHandler QueueHandler { get; }
        public ILogRepository LogRepository { get; }

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
                channel.QueueDeclare(queue: QSetting.MethodName.ToString() + "_CallBack", durable: true, exclusive: false, autoDelete: false, arguments: arguments);


                for (int i = 0; i < QSetting?.CallBackMaxCallsPerInterval; i++)
                {
                    if (currentCallCount < QSetting.CallBackMaxCallsPerInterval)
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
            long pointerId = 0;
            string extraInfo = "";
            bool isNeedToLog = false;
            try
            {
                var rawmessage = channel.BasicGet(qSetting.MethodName.ToString() + "_CallBack", false);
                if (rawmessage == null)
                {
                    return;
                }
                var body = rawmessage.Body.ToArray();
                var callCount = (int)rawmessage.BasicProperties.Headers["CallCount"];
                if (callCount >= qSetting.CallBackMaxCallCount && qSetting.CallBackMaxCallCount != -1)
                {
                    isNeedToLog = true;
                    extraInfo = "Max Call Reached. Throw Out Of The Queue.";
                    channel.BasicAck(rawmessage.DeliveryTag, multiple: false);
                    return;
                }

                var message = Encoding.UTF8.GetString(body);
                var requestBody = JsonConvert.DeserializeObject(message);
                try
                {
                    var result = await SendToCallBack(requestBody, qSetting.CallBackAddress);
                    if (result != null && result.Success)
                    {
                        isNeedToLog = true;
                        extraInfo = "CallBack Called and Success.";
                        channel.BasicAck(rawmessage.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        isNeedToLog = true;
                        extraInfo = "CallBack Called And Faild. Request Back To CallBack Queue.";
                        await BackToEndOfTheQueue(channel, rawmessage, message, qSetting.MethodName, callCount);
                    }
                }
                catch (Exception ex)
                {
                    isNeedToLog = true;
                    extraInfo = "CallBack Called And Exception Happend. Request Back To CallBack Queue.";
                    await BackToEndOfTheQueue(channel, rawmessage, message, qSetting.MethodName, callCount);
                }
            }
            finally
            {
                Interlocked.Decrement(ref currentCallCount);
                if (isNeedToLog)
                {
                    await LogRepository.Log(new QueueLog
                    {
                        ExtraInformation = extraInfo,
                        MethodName = qSetting.MethodName,
                        PointerId = pointerId,
                        QueueState = "Send To SR"

                    });
                }
                await Task.FromResult(true);
            }
        }

        private async Task<CallBackResponse> SendToCallBack(object? requestBody, string? callBackAddress)
        {
            RestCaller restCaller = new RestCaller(new RestStaticProperties
            {
                BaseAddress = callBackAddress,
                Timeout = TimeSpan.FromSeconds(90)
            });
            var result = restCaller.CallRestService<CallBackResponse>(new RestRequest_VM
            {
                Body = new JsonBody(requestBody),
                HttpMethod = HttpMethod.Post,
                EnsureSuccessStatusCode = true,
            });
            return result.GetResponse();
        }

        private static void BackToFrontOfTheQueue(IModel channel, BasicGetResult rawmessage)
        {
            channel.BasicNack(rawmessage.DeliveryTag, multiple: false, requeue: true);
        }

        private async Task BackToEndOfTheQueue(IModel channel, BasicGetResult rawmessage, string serializedRequest, MethodName methodName, int callCount)
        {
            channel.BasicNack(rawmessage.DeliveryTag, multiple: false, requeue: false);
            await QueueHandler.InsertInQueueCallBack(serializedRequest, methodName, callCount + 1);
        }
    }
}
