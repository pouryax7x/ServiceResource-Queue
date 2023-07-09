using ServiceResource.Enums;

namespace ServiceResource.Dto
{
    public class DeleteQueueMemberRequest
    {
        public MethodName MethodName { get; set; }
        public long PointerId { get; set; }
        public string Password { get; set; }
    }
}
