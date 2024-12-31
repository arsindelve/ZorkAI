namespace Utilities;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}