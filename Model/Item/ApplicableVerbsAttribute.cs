using System.Reflection;

namespace Model.Item;

/// <summary>
/// Represents an attribute used to associate a "main verb" with an interface, allowing
/// it to define the primary action verb relevant to its purpose or behavior.
/// </summary>
public class ApplicableVerbsAttribute(params string[] verbs) : Attribute
{
    private string[] Verb { get; } = verbs;

    public static List<string> GetAvailableActions(IEnumerable<IItem> items)
    {
        return items
            .SelectMany(item => item.GetType().GetInterfaces()
                .Select(i => i.GetCustomAttribute<ApplicableVerbsAttribute>())
                .Where(attr => attr != null)
                .SelectMany(attr => attr!.Verb.Select(verb => $"{verb} {item.Name}")))
            .ToList();
    }
}