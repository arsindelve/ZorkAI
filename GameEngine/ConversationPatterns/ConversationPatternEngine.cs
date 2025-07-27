using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using System.Diagnostics;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Engine for processing conversation patterns
/// </summary>
public class ConversationPatternEngine
{
    private readonly List<IConversationPattern> _patterns;

    public ConversationPatternEngine()
    {
        _patterns = ConversationPatternFactory.CreateAllPatterns();
    }

    /// <summary>
    /// Processes user input against all conversation patterns
    /// </summary>
    /// <param name="input">User input</param>
    /// <param name="context">Game context</param>
    /// <param name="client">Generation client</param>
    /// <returns>Response from matching pattern or null if no match</returns>
    public async Task<string?> ProcessInput(string input, IContext context, IGenerationClient client)
    {
        // Collect all talkable entities
        var talkables = new List<ICanBeTalkedTo>();
        talkables.AddRange(context.Items.OfType<ICanBeTalkedTo>());

        if (context.CurrentLocation is ICanContainItems container)
        {
            talkables.AddRange(container.Items.OfType<ICanBeTalkedTo>());
        }

        // Log how many talkables we have for debugging
        Debug.WriteLine($"ConversationPatternEngine processing '{input}' with {talkables.Count} talkables");
        foreach (var talkable in talkables)
        {
            if (talkable is IItem item)
            {
                Debug.WriteLine($"Available talkable: {item.GetType().Name}, Nouns: [{string.Join(", ", item.NounsForMatching)}]");
            }
        }

        // Convert input to lowercase once for efficiency
        string inputLower = input.ToLowerInvariant();

        // Try each pattern
        foreach (var pattern in _patterns)
        {
            var patternName = pattern.GetType().Name;
            Debug.WriteLine($"Trying pattern: {patternName}");

            var result = await pattern.TryMatch(input, inputLower, talkables, context, client);

            if (result != null)
            {
                Debug.WriteLine($"Pattern {patternName} matched successfully");
                return result;
            }
        }

        Debug.WriteLine("No pattern matched the input");
        return null;
    }
}
