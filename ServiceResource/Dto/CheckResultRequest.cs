using ServiceResource.Enums;

namespace ServiceResource.Dto
{
    public class CheckResultRequest
    {
        public object Response { get; set; }
        public MethodName MethodName { get; set; }
        public Exception Exception { get; set; }
        public bool Success { get; set; }
    }
}
