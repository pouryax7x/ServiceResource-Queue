using ServiceResource.Enums;

namespace ServiceResource.Persistence.Log.Entities
{
    public class QueueLog
    {
        public int Id { get; set; }
        public required MethodName MethodName { get; set; }
        public long PointerId { get; set; }
        public string QueueState { get; set; }
        public string ExtraInformation { get; set; }
        public required DateTime InsertDate { get; set; }
    }
}
