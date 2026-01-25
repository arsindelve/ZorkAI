using GameEngine.StaticCommand.Implementation;
using Model.Interface;

namespace UnitTests.IntentMapping;

/// <summary>
/// Factory that creates IGlobalCommand instances from processor type names.
/// Maps string names from JSON configuration to actual processor instances.
/// </summary>
public static class ProcessorFactory
{
    // Only processors that implement IGlobalCommand
    private static readonly Dictionary<string, Func<IGlobalCommand>> ProcessorCreators = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LookProcessor"] = () => new LookProcessor(),
        ["InventoryProcessor"] = () => new InventoryProcessor(),
        ["TakeEverythingProcessor"] = () => new TakeEverythingProcessor(),
        ["DropEverythingProcessor"] = () => new DropEverythingProcessor(),
        ["WaitProcessor"] = () => new WaitProcessor(),
        ["ScoreProcessor"] = () => new ScoreProcessor(),
        ["CurrentTimeProcessor"] = () => new CurrentTimeProcessor(),
        ["GodModeProcessor"] = () => new GodModeProcessor(),
    };

    /// <summary>
    /// Creates a new instance of the specified processor type.
    /// </summary>
    /// <param name="processorTypeName">The processor type name (e.g., "LookProcessor")</param>
    /// <returns>A new processor instance, or null if the type is not found</returns>
    public static IGlobalCommand? CreateProcessor(string processorTypeName)
    {
        return ProcessorCreators.TryGetValue(processorTypeName, out var creator)
            ? creator()
            : null;
    }

    /// <summary>
    /// Checks if a processor type is registered.
    /// </summary>
    public static bool IsValidProcessorType(string processorTypeName)
    {
        return ProcessorCreators.ContainsKey(processorTypeName);
    }

    /// <summary>
    /// Gets all registered processor type names.
    /// </summary>
    public static IEnumerable<string> GetRegisteredProcessorTypes()
    {
        return ProcessorCreators.Keys;
    }
}
