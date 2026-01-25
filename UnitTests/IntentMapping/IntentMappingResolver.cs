using Model.Intent;

namespace UnitTests.IntentMapping;

/// <summary>
/// Provides O(1) lookup from input strings to IntentBase objects.
/// Builds a dictionary from loaded configuration for fast resolution.
/// </summary>
public class IntentMappingResolver
{
    private readonly Dictionary<string, Func<IntentBase>> _intentFactories;

    /// <summary>
    /// Creates a new resolver from the specified configuration.
    /// </summary>
    public IntentMappingResolver(IntentMappingConfiguration config)
    {
        _intentFactories = new Dictionary<string, Func<IntentBase>>(StringComparer.OrdinalIgnoreCase);
        BuildLookupTable(config);
    }

    /// <summary>
    /// Creates a resolver for a specific game, loading configuration automatically.
    /// </summary>
    public static IntentMappingResolver ForGame(string gameName = "base")
    {
        var config = IntentMappingLoader.LoadConfiguration(gameName);
        return new IntentMappingResolver(config);
    }

    /// <summary>
    /// Attempts to resolve an input string to an IntentBase object.
    /// </summary>
    /// <param name="input">The user input string</param>
    /// <returns>The resolved intent, or null if no mapping exists</returns>
    public IntentBase? Resolve(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return null;

        return _intentFactories.TryGetValue(input, out var factory)
            ? factory()
            : null;
    }

    /// <summary>
    /// Checks if an input string has a mapping.
    /// </summary>
    public bool HasMapping(string input)
    {
        return _intentFactories.ContainsKey(input);
    }

    /// <summary>
    /// Gets the total number of mappings.
    /// </summary>
    public int MappingCount => _intentFactories.Count;

    private void BuildLookupTable(IntentMappingConfiguration config)
    {
        // Register global commands
        foreach (var mapping in config.GlobalCommands)
        {
            var processor = ProcessorFactory.CreateProcessor(mapping.ProcessorType);
            if (processor == null)
                continue;

            foreach (var input in mapping.Inputs)
            {
                // Capture processor type for factory (create new instance each time)
                var processorType = mapping.ProcessorType;
                _intentFactories[input] = () => new GlobalCommandIntent
                {
                    Command = ProcessorFactory.CreateProcessor(processorType)!
                };
            }
        }

        // Register simple intents
        foreach (var mapping in config.SimpleIntents)
        {
            foreach (var input in mapping.Inputs)
            {
                // Capture values for closure
                var verb = mapping.Verb;
                var noun = mapping.Noun;
                var adverb = mapping.Adverb;
                var adjective = mapping.Adjective;
                var originalInput = mapping.OriginalInput ?? input;

                _intentFactories[input] = () => new SimpleIntent
                {
                    Verb = verb,
                    Noun = noun,
                    Adverb = adverb ?? string.Empty,
                    Adjective = adjective ?? string.Empty,
                    OriginalInput = originalInput
                };
            }
        }

        // Register multi-noun intents
        foreach (var mapping in config.MultiNounIntents)
        {
            foreach (var input in mapping.Inputs)
            {
                var verb = mapping.Verb;
                var nounOne = mapping.NounOne;
                var nounTwo = mapping.NounTwo;
                var preposition = mapping.Preposition;
                var originalInput = mapping.OriginalInput ?? input;

                _intentFactories[input] = () => new MultiNounIntent
                {
                    Verb = verb,
                    NounOne = nounOne,
                    NounTwo = nounTwo,
                    Preposition = preposition,
                    OriginalInput = originalInput
                };
            }
        }

        // Register enter sub-location intents
        foreach (var mapping in config.EnterSubLocationIntents)
        {
            foreach (var input in mapping.Inputs)
            {
                var noun = mapping.Noun;

                _intentFactories[input] = () => new EnterSubLocationIntent
                {
                    Noun = noun
                };
            }
        }

        // Register exit sub-location intents
        foreach (var mapping in config.ExitSubLocationIntents)
        {
            foreach (var input in mapping.Inputs)
            {
                var nounOne = mapping.NounOne;
                var nounTwo = mapping.NounTwo;

                _intentFactories[input] = () => new ExitSubLocationIntent
                {
                    NounOne = nounOne,
                    NounTwo = nounTwo
                };
            }
        }
    }
}
