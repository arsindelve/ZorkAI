using System.Reflection;

namespace Model.Item;

/// <summary>
/// Represents an attribute used to associate a "main verb" with an interface, allowing
/// it to define the primary action verb relevant to its purpose or behavior.
/// </summary>
public class ApplicableVerbsAttribute(params string[] verbs) : Attribute
{
    private string[] Verb { get; } = verbs;

    /// <summary>
    /// Retrieves a dictionary of available actions for a collection of items,
    /// excluding specific actions based on the provided exclusions.
    /// </summary>
    /// <param name="items">A collection of items implementing the IItem interface.</param>
    /// <param name="exclusions">An array of action strings to be excluded from the results.</param>
    /// <returns>A dictionary where the key is the item name and the value is a list of applicable action strings
    /// associated with the item.</returns>
    public static Dictionary<string, List<string>> GetAvailableActions(IEnumerable<IItem> items,
        params string[] exclusions)
    {
        List<IItem> itemsList = new();

        foreach (var item in items.ToList())
        {
            if (itemsList.All(i => i.GetType() != item.GetType()))
                itemsList.Add(item);
        }

        return itemsList
            .ToDictionary(
                item => item.Name,
                item => item.GetType().GetInterfaces()
                    .Select(i => i.GetCustomAttribute<ApplicableVerbsAttribute>())
                    .Where(attr => attr != null)
                    .SelectMany(attr => attr!.Verb
                        .Where(verb => !exclusions.Contains(verb))
                        .Select(verb => $"{verb} {item.Name}"))
                    .Distinct()
                    .ToList());
    }
}