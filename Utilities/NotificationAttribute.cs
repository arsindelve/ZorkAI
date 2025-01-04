namespace Utilities;

[AttributeUsage(AttributeTargets.Field)]
public class NotificationAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}