namespace Utilities;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class NotificationAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}