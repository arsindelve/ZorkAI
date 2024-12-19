using CloudWatch.Model;

namespace CloudWatch;

public static class CloudWatchLoggerFactory
{
    public static async Task<ICloudWatchLogger<T>> Get<T>(string groupName, string streamName, Guid turnCorrelationId)
        where T : ITurnBasedLog
    {
        var logger = new CloudWatchLogger<T>(groupName, streamName, turnCorrelationId);
        await logger.CreateLogGroupIfNotExists();
        await logger.CreateLogStreamIfNotExists();
        return logger;
    }
}