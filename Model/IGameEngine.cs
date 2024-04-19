namespace Model;

public interface IGameEngine
{
    /// <summary>
    ///     Represents the number of moves made in the game.
    /// </summary>
    /// <remarks>
    ///     The "Moves" property counts the number of moves made by the player in the game.
    ///     Each move can include actions such as moving to a different location, picking up an item, or interacting
    ///     with the game environment.
    /// </remarks>
    int Moves { get; }

    /// <summary>
    ///     Represents the score of a game.
    /// </summary>
    /// <remarks>
    ///     The score is a measure of the player's progress or success in the game.
    ///     It indicates the number of points or rewards earned by the player.
    ///     Higher score generally means better performance or achievement.
    /// </remarks>
    int Score { get; }

    /// <summary>
    ///     Represents the name of the location in the game.
    /// </summary>
    string LocationName { get; }

    string IntroText { get; }
    
    /// <summary>
    /// Indicates the runtime container....is it Web, Console, other? 
    /// </summary>
    Runtime Runtime { get; set; }

    /// <summary>
    ///     Retrieves the game engine's response based on the player's input.
    /// </summary>
    /// <param name="playerInput">The player's input.</param>
    /// <returns>The game engine's response as a string.</returns>
    Task<string?> GetResponse(string? playerInput);

    /// <summary>
    ///     Restores a game from the provided data.
    /// </summary>
    /// <param name="data">The serialized game data.</param>
    /// <returns>The restored game context.</returns>
    IContext RestoreGame(string data);

    /// <summary>
    ///     Saves the current game state.
    /// </summary>
    /// <returns>The serialized game data.</returns>
    string SaveGame();
}