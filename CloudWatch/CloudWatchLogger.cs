using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using CloudWatch.Model;
using Newtonsoft.Json;

namespace CloudWatch;

public class CloudWatchLogger<T>(string groupName, string streamName, Guid turnCorrelationId) : ICloudWatchLogger<T> 
    where T : ITurnBasedLog
{
    private readonly AmazonCloudWatchLogsClient _client = new(RegionEndpoint.USEast1);

    internal async Task CreateLogGroupIfNotExists()
    {
        try
        {
            await _client.CreateLogGroupAsync(new CreateLogGroupRequest
            {
                LogGroupName = groupName
            });
        }
        catch (ResourceAlreadyExistsException)
        {
            Console.WriteLine($"Log group '{groupName}' already exists.");
        }
    }

    internal async Task CreateLogStreamIfNotExists()
    {
        try
        {
            await _client.CreateLogStreamAsync(new CreateLogStreamRequest
            {
                LogGroupName = groupName,
                LogStreamName = streamName
            });
            Console.WriteLine($"Log stream '{streamName}' created.");
        }
        catch (ResourceAlreadyExistsException)
        {
            Console.WriteLine($"Log stream '{streamName}' already exists.");
        }
    }

    public async Task WriteLogEvents(T log) 
    {
        log.TurnCorrelationId = turnCorrelationId.ToString();
        string logString = JsonConvert.SerializeObject(log);
        
        var logEvents = new List<InputLogEvent>
        {
            new()
            {
                Message = logString,
                Timestamp = DateTime.UtcNow
            }
        };

        await _client.PutLogEventsAsync(new PutLogEventsRequest
        {
            LogGroupName = groupName,
            LogStreamName = streamName,
            LogEvents = logEvents
        });
    }
}