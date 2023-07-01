using ServiceResource.Enums;
using System.ComponentModel.DataAnnotations;

namespace ServiceResource.Persistence.Log.Entities
{
    public class ResponseLog
    {
        [Key]
        public long Id { get; set; }
        public required MethodName MethodName { get; set; }
        public required string Input { get; set; }
        public required DateTime CallTime { get; set; }
        public string? SummeryData { get; set; }
        public required long PointerId { get; set; }
        public required string Output { get; set; }
        public string? Exception { get; set; }
        public required int ErrorCode { get; set; }
        public long RequestId { get; set; }
        public required DateTime ResponseTime { get; set; }
        public string? UserData { get; set; }

    }
}
