using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using RabbitMQ.Client;
using ServiceResource.Dto;
using ServiceResource.Enums;
using ServiceResource.Interfaces;
using ServiceResource.Persistence.Queue.Entities;
using System.Text;
using System.Threading.Channels;
namespace ServiceResource.Business.Queue
{
    public class QueueHandler : IQueueHandler
    {
        public IQueueContext QueueContext { get; }
        public IQueueRepository QueueRepository { get; }

        public QueueHandler(IQueueRepository queueRepository)
        {
            QueueRepository = queueRepository;
        }
        public async Task<bool> InsertInQueue(SRRequest request, int callCount = 0)
        {
            if (IsItReadyToSendToCallBack(request))
            {
                return await InsertInQueueCallBack(request.QueueSetting.SerializedOutput, request.MethodName, callCount);
            }
            var queueSetting = await QueueRepository.GetQueueSetting(request.MethodName);

            using (var connection = QueueRepository.GetFactory().CreateConnection())
            using (var channel = connection.CreateModel())
            {

                var arguments = new Dictionary<string, object>
                    {
                        { "x-message-deduplication", true } // Enable message deduplication for the queue
                    };
                channel.ConfirmSelect(); // Enable publisher confirmations

                string QueueName = queueSetting.MethodName.ToString();



                channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);


                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                // Add a custom header for the request identifier
                properties.Headers = new Dictionary<string, object>
                    {
                        { "CallCount", callCount },
                        { "PointerId", request.PointerId },
                    };

                // Add header for dublication
                if (request.QueueSetting.PreventDuplicate)
                {
                    properties.Headers.Add("x-deduplication-header", request.PointerId);
                }

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));


                channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: properties, body: body);

                return channel.WaitForConfirms(TimeSpan.FromSeconds(10)); // Wait for confirmation result
            }
        }

        public async Task<bool> InsertInQueueCallBack(string serilizedRequest, MethodName methodName, int callCount = 0)
        {
            var queueSetting = await QueueRepository.GetQueueSetting(methodName);

            using (var connection = QueueRepository.GetFactory().CreateConnection())
            using (var channel = connection.CreateModel())
            {

                var arguments = new Dictionary<string, object>
                    {
                        { "x-message-deduplication", true } // Enable message deduplication for the queue
                    };
                channel.ConfirmSelect(); // Enable publisher confirmations

                string QueueName = queueSetting.MethodName.ToString() + "_CallBack";

                channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);


                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                // Add a custom header for the request identifier
                properties.Headers = new Dictionary<string, object>
                    {
                        { "CallCount", callCount },
                    };

                //// Add header for dublication
                //if (request.QueueSetting.PreventDuplicate)
                //{
                //    properties.Headers.Add("x-deduplication-header", request.PointerId);
                //}
                var body = Encoding.UTF8.GetBytes(serilizedRequest);

                channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: properties, body: body);

                return channel.WaitForConfirms(TimeSpan.FromSeconds(10)); // Wait for confirmation result
            }


        }
        private static bool IsItReadyToSendToCallBack(SRRequest request)
        {
            return !string.IsNullOrEmpty(request?.QueueSetting?.SerializedOutput);
        }

        public async Task<bool> DeleteAllQueueMembers(DeleteQueueMembersRequest request)
        {
            if (!CheckDeleteQueueMemberPassword(request.Password))
            {
                return false;
            }
            var queueSetting = await QueueRepository.GetQueueSetting(request.MethodName);
            using (var connection = QueueRepository.GetFactory().CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    // Delete all messages in the queue
                    channel.QueuePurge(queueSetting.MethodName.ToString());
                }
            }
            return true;
        }

        public async Task<bool> DeleteQueueMember(DeleteQueueMemberRequest request)
        {
            if (!CheckDeleteQueueMemberPassword(request.Password))
            {
                return false;
            }
            bool messageDeleted = false;
            var queueSetting = await QueueRepository.GetQueueSetting(request.MethodName);
            using (var connection = QueueRepository.GetFactory().CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var arguments = new Dictionary<string, object>
                    {
                        { "x-message-deduplication", true } // Enable message deduplication for the queue
                    };
                    string QueueName = queueSetting.MethodName.ToString();

                    channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);

                    var result = channel.BasicGet(QueueName, autoAck: false);

                    while (result != null)
                    {
                        var headers = result.BasicProperties.Headers;
                        var PointerId = (long)result.BasicProperties.Headers["PointerId"];

                        if (PointerId == request.PointerId)
                        {
                            channel.BasicAck(result.DeliveryTag, false);
                            messageDeleted = true;
                            break;
                        }
                        result = channel.BasicGet(QueueName, autoAck: false);
                    }
                }
            }
            return messageDeleted;
        }

        private bool CheckDeleteQueueMemberPassword(string password)
        {
            if (password == "@@#M@hsa@M!N!")
            {
                return true;
            }
            return false;
        }
    }
}
