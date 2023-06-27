using ServiceResource.Enums;

namespace ServiceResource.Dto
{
    public class Mock
    {
        public ExpectedAnswer ExpectedAnswer { get; set; }
        public object Response { get; set; }
        public Exception Exception { get; set; }
    }
}
