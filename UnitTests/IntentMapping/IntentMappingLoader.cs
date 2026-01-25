using System.Reflection;
using System.Text.Json;

namespace UnitTests.IntentMapping;

/// <summary>
/// Loads intent mapping configurations from embedded JSON resources.
/// Supports inheritance via the "extends" field for game-specific overlays.
/// </summary>
public static class IntentMappingLoader
{
    private static readonly Dictionary<string, IntentMappingConfiguration> ConfigCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Loads and merges intent mapping configuration for a specific game.
    /// If the game config extends another, the base is loaded first and merged.
    /// </summary>
    /// <param name="gameName">The game name (e.g., "base", "zorkone", "planetfall")</param>
    /// <returns>The merged configuration</returns>
    public static IntentMappingConfiguration LoadConfiguration(string gameName = "base")
    {
        var normalizedName = gameName.ToLowerInvariant();

        if (ConfigCache.TryGetValue(normalizedName, out var cached))
            return cached;

        var config = LoadConfigurationInternal(normalizedName);
        ConfigCache[normalizedName] = config;
        return config;
    }

    /// <summary>
    /// Clears the configuration cache. Useful for testing.
    /// </summary>
    public static void ClearCache()
    {
        ConfigCache.Clear();
    }

    private static IntentMappingConfiguration LoadConfigurationInternal(string gameName)
    {
        var resourceName = $"UnitTests.IntentMappings.{gameName}-mappings.json";
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            // If no game-specific config, try to load base
            if (gameName != "base")
                return LoadConfiguration("base");

            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        var config = JsonSerializer.Deserialize<IntentMappingConfiguration>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize configuration from {resourceName}");

        // Handle inheritance
        if (!string.IsNullOrEmpty(config.Extends))
        {
            var baseConfig = LoadConfiguration(config.Extends);
            config = MergeConfigurations(baseConfig, config);
        }

        return config;
    }

    /// <summary>
    /// Merges a derived configuration on top of a base configuration.
    /// Derived entries override base entries with the same inputs.
    /// </summary>
    private static IntentMappingConfiguration MergeConfigurations(
        IntentMappingConfiguration baseConfig,
        IntentMappingConfiguration derivedConfig)
    {
        var merged = new IntentMappingConfiguration
        {
            Version = derivedConfig.Version,
            GameName = derivedConfig.GameName,
            Extends = derivedConfig.Extends
        };

        // Merge global commands - derived overrides base
        var globalCommandsByInput = new Dictionary<string, GlobalCommandMapping>(StringComparer.OrdinalIgnoreCase);
        foreach (var cmd in baseConfig.GlobalCommands)
            foreach (var input in cmd.Inputs)
                globalCommandsByInput[input] = cmd;
        foreach (var cmd in derivedConfig.GlobalCommands)
            foreach (var input in cmd.Inputs)
                globalCommandsByInput[input] = cmd;
        merged.GlobalCommands = globalCommandsByInput.Values.Distinct().ToList();

        // Merge simple intents
        var simpleByInput = new Dictionary<string, SimpleIntentMapping>(StringComparer.OrdinalIgnoreCase);
        foreach (var intent in baseConfig.SimpleIntents)
            foreach (var input in intent.Inputs)
                simpleByInput[input] = intent;
        foreach (var intent in derivedConfig.SimpleIntents)
            foreach (var input in intent.Inputs)
                simpleByInput[input] = intent;
        merged.SimpleIntents = simpleByInput.Values.Distinct().ToList();

        // Merge multi-noun intents
        var multiByInput = new Dictionary<string, MultiNounIntentMapping>(StringComparer.OrdinalIgnoreCase);
        foreach (var intent in baseConfig.MultiNounIntents)
            foreach (var input in intent.Inputs)
                multiByInput[input] = intent;
        foreach (var intent in derivedConfig.MultiNounIntents)
            foreach (var input in intent.Inputs)
                multiByInput[input] = intent;
        merged.MultiNounIntents = multiByInput.Values.Distinct().ToList();

        // Merge enter sub-location intents
        var enterByInput = new Dictionary<string, EnterSubLocationMapping>(StringComparer.OrdinalIgnoreCase);
        foreach (var intent in baseConfig.EnterSubLocationIntents)
            foreach (var input in intent.Inputs)
                enterByInput[input] = intent;
        foreach (var intent in derivedConfig.EnterSubLocationIntents)
            foreach (var input in intent.Inputs)
                enterByInput[input] = intent;
        merged.EnterSubLocationIntents = enterByInput.Values.Distinct().ToList();

        // Merge exit sub-location intents
        var exitByInput = new Dictionary<string, ExitSubLocationMapping>(StringComparer.OrdinalIgnoreCase);
        foreach (var intent in baseConfig.ExitSubLocationIntents)
            foreach (var input in intent.Inputs)
                exitByInput[input] = intent;
        foreach (var intent in derivedConfig.ExitSubLocationIntents)
            foreach (var input in intent.Inputs)
                exitByInput[input] = intent;
        merged.ExitSubLocationIntents = exitByInput.Values.Distinct().ToList();

        return merged;
    }
}
