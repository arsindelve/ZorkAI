using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Defines a pattern for recognizing and handling conversational commands
/// </summary>
public interface IConversationPattern
{
    /// <summary>
    /// Attempts to match a user input against this conversation pattern
    /// </summary>
    /// <param name="input">The original user input</param>
    /// <param name="inputLower">The lowercase version of the input</param>
    /// <param name="talkables">List of entities that can be talked to</param>
    /// <param name="context">The game context</param>
    /// <param name="client">The generation client</param>
    /// <returns>The conversation result if this pattern matched, otherwise null</returns>
    Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client);
}
