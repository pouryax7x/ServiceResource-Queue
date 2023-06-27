namespace ServiceResource.Dto
{
    public class ServiceQueue
    {
        public string ServiceName { get; set; }
        public string QueueName { get; set; }
        public int MaxCallsPerInterval { get; set; }
        public TimeSpan Interval { get; set; }
        public DateTime LastCallTime { get; set; }
    }
}
