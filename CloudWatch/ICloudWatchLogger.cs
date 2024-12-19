namespace CloudWatch;

public interface ICloudWatchLogger<T>
{
    Task WriteLogEvents(T log);
}