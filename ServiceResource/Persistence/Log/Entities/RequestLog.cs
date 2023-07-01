using ServiceResource.Enums;
using System.ComponentModel.DataAnnotations;

namespace ServiceResource.Persistence.Log.Entities
{
    public class RequestLog
    {
        [Key]
        public long Id { get; set; }
        public required MethodName MethodName { get; set; }
        public required string Input { get; set; }
        public required DateTime CallTime { get; set; }
        public string? SummeryData { get; set; }
        public required long PointerId { get; set; }
    }
}
