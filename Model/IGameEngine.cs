namespace Model;

public interface IGameEngine
{
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
    void RegisterActor(ITurnBasedActor actor);

    /// <summary>
    ///     Removes an actor from the game engine. Once removed,
    ///     the actor will no longer act every turn.
    /// </summary>
    /// <param name="actor">The actor to be removed.</param>
    void RemoveActor(ITurnBasedActor actor);
}