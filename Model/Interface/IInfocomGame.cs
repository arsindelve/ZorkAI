namespace Model.Interface;

/// <summary>
///     Represents the details of an Infocom game that the game engine can
///     use to get game specific information like the starting location
/// </summary>
public interface IInfocomGame
{
    /// <summary>
    ///     Represents the starting location of an Infocom game.
    /// </summary>
    Type StartingLocation { get; }

    string GameName { get; }

    /// <summary>
    /// Represents the base AI generative system prompt for an Infocom game.
    /// </summary>
    /// <remarks>
    /// The system prompt is the text that the game engine uses to deliver responses as the invisible, incorporeal Narrator within the game.
    /// This text is typically one or two sentences that do not progress the story but keep the player engaged.
    /// The system prompt should not give suggestions or hints about the game, and should remind players that it is a game.
    /// </remarks>
    /// <value>
    /// The text of the system prompt.
    /// </value>
    string SystemPromptSecretKey { get; }

    string StartText { get; }

    /// <summary>
    ///     Represents the default save game name for an Infocom game.
    /// </summary>
    /// <remarks>
    ///     This property specifies the default name to be used when saving the game in an Infocom game.
    /// </remarks>
    /// <value>
    ///     The default save game name.
    /// </value>
    string DefaultSaveGameName { get; }

    /// <summary>
    /// Represents the DynamoDb session table name of an Infocom game. The session table
    /// name is used to store and retrieve game session data in the database.
    /// </summary>
    /// <remarks>
    /// The session table name should be a unique identifier for each game,
    /// ensuring that the session data is stored and retrieved correctly.
    /// </remarks>
    string SessionTableName { get; }

    /// <summary>
    ///     Retrieves the description of a score for the current game.
    /// </summary>
    /// <param name="score">The score to get the description for.</param>
    /// <returns>The description of the score.</returns>
    string GetScoreDescription(int score);

    IGlobalCommandFactory GetGlobalCommandFactory();

    /// <summary>
    /// Initializes the game context with the provided <see cref="IContext"/>.
    /// </summary>
    /// <param name="context">The game context to be initialized.</param>
    void Init(IContext context);

    /// <summary>
    /// The talkable NPCs (<c>ICanBeTalkedTo</c>) this game knows about regardless of where they
    /// currently are. Used so that directly addressing an absent named character (e.g.
    /// "Floyd, go up") is recognized and answered with "X isn't here." instead of letting the
    /// command leak into normal player parsing (which would move the player or drop their items).
    /// Listing the types here also makes a not-yet-instantiated NPC "known" despite lazy loading.
    /// Defaults to none.
    /// </summary>
    IReadOnlyList<Type> TalkableCharacterTypes => [];
}