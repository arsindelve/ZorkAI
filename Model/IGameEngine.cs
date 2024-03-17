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
}