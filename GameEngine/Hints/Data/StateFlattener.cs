using System.Collections;
using System.Reflection;
using Model.Interface;
using Model.Item;
using Model.Location;
using Newtonsoft.Json;

namespace GameEngine.Hints.Data;

/// <summary>
///     Reads the whole live game world — every item and location in the Repository — into a flat map of
///     "Item:Magnet.HasEverBeenPickedUp" -> "True". Only persisted state is read (public properties with a
///     public setter, not [JsonIgnore]), of scalar-ish types: bools, integers, strings, enums, references to
///     other game objects (as the referenced type's name, or "Player" for the adventurer), and string lists
///     (sorted, joined with '|'). The generator diffs these maps between walkthrough steps to find each
///     puzzle's completion signals; the data-driven graph evaluates the same map at hint time.
/// </summary>
public static class StateFlattener
{
    public const string Player = "Player";
    public const string Null = "null";

    public static IReadOnlyDictionary<string, string> Flatten(IContext context)
    {
        var flat = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in Repository.GetAllItems())
            Add(flat, "Item:" + item.GetType().Name, item, context);
        foreach (var location in Repository.GetAllLocations())
            Add(flat, "Location:" + location.GetType().Name, location, context);
        return flat;
    }

    /// <summary>The nouns each flattened object answers to, keyed like the paths' object part ("Item:Magnet").</summary>
    public static IReadOnlyDictionary<string, string[]> Nouns()
    {
        var nouns = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var item in Repository.GetAllItems())
            nouns["Item:" + item.GetType().Name] = item.NounsForMatching.ToArray(); // (Name throws for noun-less timers)
        foreach (var location in Repository.GetAllLocations())
            nouns["Location:" + location.GetType().Name] = new[] { location.Name };
        return nouns;
    }

    private static void Add(Dictionary<string, string> flat, string prefix, object obj, IContext context)
    {
        foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0) continue;
            if (property.SetMethod is null || !property.SetMethod.IsPublic) continue;
            if (property.GetCustomAttribute<JsonIgnoreAttribute>() is not null) continue;

            object? value;
            try
            {
                value = property.GetValue(obj);
            }
            catch (Exception)
            {
                continue; // a getter that throws is not state
            }

            var text = Stringify(value, property.PropertyType, context);
            if (text is not null)
                flat[prefix + "." + property.Name] = text;
        }
    }

    private static string? Stringify(object? value, Type declared, IContext context)
    {
        var type = Nullable.GetUnderlyingType(declared) ?? declared;

        if (type == typeof(bool) || type == typeof(int) || type == typeof(byte) || type == typeof(long) ||
            type == typeof(short) || type == typeof(string) || type.IsEnum)
            return value?.ToString() ?? Null;

        // String lists carry a marker so a diff can tell "elements were added" from a string that changed.
        if (typeof(IEnumerable<string>).IsAssignableFrom(type))
            return "list:" + (value is IEnumerable<string> list ? string.Join("|", list.OrderBy(s => s, StringComparer.Ordinal)) : "");

        if (typeof(IContext).IsAssignableFrom(type) || typeof(ICanContainItems).IsAssignableFrom(type) ||
            typeof(IItem).IsAssignableFrom(type) || typeof(ILocation).IsAssignableFrom(type))
        {
            if (value is null) return Null;
            return ReferenceEquals(value, context) || value is IContext ? Player : value.GetType().Name;
        }

        if (value is IEnumerable) return null; // other collections are not scalar state
        return null;
    }
}
