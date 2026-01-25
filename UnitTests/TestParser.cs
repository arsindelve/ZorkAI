using GameEngine;
using GameEngine.StaticCommand.Implementation;
using Model.AIParsing;
using Model.Intent;
using Model.Interface;
using UnitTests.IntentMapping;

namespace UnitTests;

/// <summary>
///     Represents a unit test appropriate implementation of the IIntentParser interface.
///     Uses JSON-based configuration for static mappings with O(1) lookup performance.
/// </summary>
public class TestParser : IntentParser
{
    private readonly string[] _allContainers;
    private readonly string[] _allNouns;
    private readonly string[] _verbs;
    private readonly IntentMappingResolver _resolver;

    public TestParser(IGlobalCommandFactory gameSpecificCommandFactory, string gameName = "ZorkOne") : base(
        Mock.Of<IAIParser>(), gameSpecificCommandFactory)
    {
        _verbs =
        [
            "take", "drop", "open", "close", "examine", "look", "eat", "press", "remove", "play", "shoot",
            "deactivate", "type", "key", "punch", "push", "pull", "burn", "set", "search", "empty", "wear",
            "drink", "use", "count", "touch", "read", "turn", "wave", "move", "ring", "activate", "search",
            "smell", "turn on", "turn off", "throw", "light", "rub", "kiss", "wind", "kick", "deflate",
            "lower", "raise", "get", "inflate", "leave", "unlock", "lock", "climb", "extend", "lift"
        ];

        _allNouns = Repository.GetNouns(gameName);
        _allContainers = Repository.GetContainers(gameName);

        IEnumerable<string> specialNouns =
        [
            "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "zero", "dial", "shelves", "pocket",
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "dude", "hello", "17", "seventeen", "slot", "lever", "label",
            "tree", "branches", "house", "lettering", "mirror", "match", "yellow button", "red button", "button", "medicine",
            "blue button", "brown button", "bolt", "bubble", "bodies", "gate", "lid", "switch", "slag", "engravings",
            "fromitz board", "board", "fromitz", "second fromitz board", "second board", "second",
            "second board", "second fromitz board", "second fromitz", "384"
        ];

        _allNouns = _allNouns.Union(specialNouns).ToArray();

        // Initialize the JSON-based resolver for O(1) lookups
        _resolver = IntentMappingResolver.ForGame("base");
    }

    public override Task<IntentBase> DetermineComplexIntentType(string? input, string locationDescription,
        string sessionId)
    {
        // 1. Try JSON-based lookup first (O(1))
        var resolved = _resolver.Resolve(input);
        if (resolved != null)
            return Task.FromResult(resolved);

        // 2. Dynamic pattern: "put X in Y" with runtime noun lookup
        if (input?.StartsWith("put") ?? false)
        {
            var putWords = input.Split(" ");
            if (putWords.Length >= 4 && _allNouns.Contains(putWords[1]) && _allContainers.Contains(putWords[3]))
                return Task.FromResult<IntentBase>(new MultiNounIntent
                {
                    NounOne = putWords[1],
                    NounTwo = putWords[3],
                    Verb = "put",
                    Preposition = "in",
                    OriginalInput = input
                });
        }

        // 3. Handle multi-word goo nouns - must handle both adjective+noun and just "goo"
        if (input?.Contains("red goo") ?? false)
        {
            var verb = input.Split(' ')[0];
            if (_verbs.Contains(verb))
                return Task.FromResult<IntentBase>(new SimpleIntent
                {
                    Verb = verb,
                    Noun = "red goo",
                    OriginalInput = input
                });
        }

        if (input?.Contains("brown goo") ?? false)
        {
            var verb = input.Split(' ')[0];
            if (_verbs.Contains(verb))
                return Task.FromResult<IntentBase>(new SimpleIntent
                {
                    Verb = verb,
                    Noun = "brown goo",
                    OriginalInput = input
                });
        }

        if (input?.Contains("green goo") ?? false)
        {
            var verb = input.Split(' ')[0];
            if (_verbs.Contains(verb))
                return Task.FromResult<IntentBase>(new SimpleIntent
                {
                    Verb = verb,
                    Noun = "green goo",
                    OriginalInput = input
                });
        }

        // Handle just "goo" (defaults to first one found)
        if (input?.Split(' ').Contains("goo") ?? false)
        {
            var verb = input.Split(' ')[0];
            if (_verbs.Contains(verb))
                return Task.FromResult<IntentBase>(new SimpleIntent
                {
                    Verb = verb,
                    Noun = "goo",
                    OriginalInput = input
                });
        }

        // 4. Fallback: verb+noun matching after removing "the"
        input = input?.Replace("the ", "").Trim();
        var words = input?.Split(" ");

        if (words?.Length >= 2 && _verbs.Contains(words[0]) && _allNouns.Contains(words[1]))
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Verb = words[0],
                Noun = words[1],
                OriginalInput = input
            });

        return Task.FromResult<IntentBase>(new NullIntent());
    }

    public override Task<string?> ResolvePronounsAsync(string input, string? lastInput, string? lastResponse)
    {
        // Simple test-mode pronoun resolution
        var lower = input.ToLowerInvariant();
        var lastInputLower = lastInput?.ToLowerInvariant() ?? string.Empty;
        var lastResponseLower = lastResponse?.ToLowerInvariant() ?? string.Empty;

        // "turn it on" after "take lamp" - pronoun refers to player's previous input
        if (lower.Contains("turn") && lower.Contains("it") && lastInputLower.Contains("lamp"))
            return Task.FromResult<string?>("turn lamp on");

        // "open it" after door mentioned in response
        if (lower.Contains("open it") && lastResponseLower.Contains("door"))
            return Task.FromResult<string?>("open door");

        // "open it" after bulkhead mentioned in response
        if (lower.Contains("open it") && lastResponseLower.Contains("bulkhead"))
            return Task.FromResult<string?>("open bulkhead");

        // Chained pronoun resolution tests: "put it on" after taking gas mask
        if ((lower.Contains("put it on") || lower.Contains("wear it")) && lastInputLower.Contains("gas mask"))
            return Task.FromResult<string?>("wear gas mask");

        // "drop it" after any gas mask interaction
        if (lower.Contains("drop it") && lastInputLower.Contains("gas mask"))
            return Task.FromResult<string?>("drop gas mask");

        // "take it" after "drop gas mask"
        if (lower.Contains("take it") && lastInputLower.Contains("drop gas mask"))
            return Task.FromResult<string?>("take gas mask");

        // Chained pronoun resolution: "drop it" after taking an item
        if (lower.Contains("drop it") && lastInputLower.Contains("take "))
        {
            // Extract the noun from "take X" in lastInput
            var match = System.Text.RegularExpressions.Regex.Match(lastInputLower, @"take\s+(.+)");
            if (match.Success)
                return Task.FromResult<string?>($"drop {match.Groups[1].Value}");
        }

        // "examine it" after taking an item
        if (lower.Contains("examine it") && lastInputLower.Contains("take "))
        {
            var match = System.Text.RegularExpressions.Regex.Match(lastInputLower, @"take\s+(.+)");
            if (match.Success)
                return Task.FromResult<string?>($"examine {match.Groups[1].Value}");
        }

        // "take it" after "open desk" reveals gas mask
        if (lower.Contains("take it") && lastResponseLower.Contains("gas mask"))
            return Task.FromResult<string?>("take gas mask");

        // General "it" resolution from lastInput noun (catches many cases)
        if (lower.Contains(" it") || lower.StartsWith("it "))
        {
            // Try to extract noun from lastInput pattern like "verb noun"
            var inputWords = lastInputLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (inputWords.Length >= 2)
            {
                // The noun could be the second word, or the rest of the string after the verb
                var potentialNoun = string.Join(" ", inputWords.Skip(1));
                var resolvedInput = System.Text.RegularExpressions.Regex.Replace(
                    lower, @"\bit\b", potentialNoun, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (resolvedInput != lower)
                    return Task.FromResult<string?>(resolvedInput);
            }
        }

        // No resolution needed
        return Task.FromResult<string?>(null);
    }
}
