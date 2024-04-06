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
    /// Retrieves the game engine's response based on the player's input.
    /// </summary>
    /// <param name="playerInput">The player's input.</param>
    /// <returns>The game engine's response as a string.</returns>
    Task<string?> GetResponse(string? playerInput);

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

    /// <summary>
    ///     Registers an actor with the game engine. Until the actor
    ///     is removed, it will act every turn.
    /// </summary>
    /// <param name="actor">The actor to be registered.</param>
    // TODO: This method should not be on the public interface. I hate it being here. 
    void RegisterActor(ITurnBasedActor actor);

    /// <summary>
    ///     Removes an actor from the game engine. Once removed,
    ///     the actor will no longer act every turn.
    /// </summary>
    /// <param name="actor">The actor to be removed.</param>
    // TODO: This method should not be on the public interface. I hate it being here. 
    void RemoveActor(ITurnBasedActor actor);
}