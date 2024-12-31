using System.Reflection;

namespace Utilities;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        // Get the field corresponding to the enum value
        var fieldInfo = value.GetType().GetField(value.ToString());
        if (fieldInfo is null) return value.ToString();

        // Fetch the DescriptionAttribute applied to it, if any
        var attribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();

        // Return the description if available, otherwise fallback to the enum value name
        return attribute?.Description ?? value.ToString();
    }
}