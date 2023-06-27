namespace ServiceResource.Enums
{
    public enum ServiceCallingMode
    {
        Immediate,
        QueueOnFaild,
        DirectlyToQueue,
        ImmediateWithCheckResult
    }
}
