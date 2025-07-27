namespace GameEngine.ConversationPatterns;

/// <summary>
/// Factory for creating conversation patterns
/// </summary>
public static class ConversationPatternFactory
{
    /// <summary>
    /// Creates a collection of all available conversation patterns
    /// </summary>
    /// <returns>A list of conversation patterns</returns>
    public static List<IConversationPattern> CreateAllPatterns()
    {
        return new List<IConversationPattern>
        {
            // Direct question patterns
            new AskAboutPattern(),
            new AskCharacterForPattern(),
            new QueryForPattern(),

            // General conversation patterns
            new CharacterCommaPattern(),
            new TalkToCharacterPattern(),
            new GreetCharacterPattern(),

            // Verb-based patterns
            new VerbCharacterPattern(),
            new VerbTextToCharacterPattern(),
            new ShowCharacterItemPattern(),
            new WhisperToCharacterPattern(),
            new InterrogateCharacterPattern()

            // Add new patterns here
        };
    }
}