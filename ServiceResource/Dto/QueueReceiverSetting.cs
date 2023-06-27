using ServiceResource.Persistence.Queue.Entities;

namespace ServiceResource.Dto
{
    public class QueueReceiverSetting : QueueSetting
    {
        public int CallCount = 0;
        public int CallBackCallCount = 0;
    }
}
