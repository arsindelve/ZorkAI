namespace Utilities;

[AttributeUsage(AttributeTargets.Field)]
public class DescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}