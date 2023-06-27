using ServiceResource.Enums;
using System.ComponentModel.DataAnnotations;

namespace ServiceResource.Dto
{
    public class SRRequest
    {
        public required MethodName MethodName { get; set; }
        public required ServiceCallingMode CallingMode { get; set; }
        public required object Input { get; set; }
        public required int SendTimeoutSecounds { get; set; }
        public long PointerId { get; set; }
        public Mock? Mock { get; set; }
        public CheckResult? CheckResult { get; set; }
        public QueueVar? QueueSetting { get; set; }
    }
}
