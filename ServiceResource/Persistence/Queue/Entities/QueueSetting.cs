using ServiceResource.Enums;
using System.ComponentModel.DataAnnotations;

namespace ServiceResource.Persistence.Queue.Entities
{
    public class QueueSetting
    {
        [Key]
        public required int Id { get; set; }
        public required MethodName MethodName { get; set; }
        public required int MaxCallsPerInterval { get; set; }
        public required int Interval_Sec { get; set; }
        public required int MaxCallCount { get; set; }
        public int CallBackMaxCallsPerInterval { get; set; }
        public int CallBackInterval_Sec { get; set; }
        public int CallBackMaxCallCount { get; set; }
        public string? CallBackAddress { get; set; }
    }
}
