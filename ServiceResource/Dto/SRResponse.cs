using Newtonsoft.Json;
using static Common;

namespace ServiceResource.Dto
{
    public class SRResponse
    {
        public bool Success { get; set; }
        public ExceptionDto? Exception { get; set; }
        public string? Message { get; set; }
        public int ErrorCode { get; set; }
        public string? Response { get; set; }

    }
    public class ExceptionDto
    {
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
        // Include any additional properties needed for serialization
    }
}
