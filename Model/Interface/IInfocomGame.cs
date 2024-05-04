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
    ///     Retrieves the description of a score for the current game.
    /// </summary>
    /// <param name="score">The score to get the description for.</param>
    /// <returns>The description of the score.</returns>
    string GetScoreDescription(int score);

    IGlobalCommandFactory GetGlobalCommandFactory();
}