using ServiceResource.Enums;
using System.Reflection.Metadata.Ecma335;

namespace ServiceResource.Dto
{
    public class DeleteQueueMembersRequest
    {
        public MethodName MethodName { get; set; }
        public string Password { get; set; }
    }
}
