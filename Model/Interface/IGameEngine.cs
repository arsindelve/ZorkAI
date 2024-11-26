using Model.AIGeneration;
using Model.Movement;

namespace Model.Interface;

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

    string SessionTableName { get; }

    /// <summary>
    ///     Represents the client responsible for generating text for different scenarios in the game that don't have
    ///     pre-determined outcomes.
    /// </summary>
    /// <remarks>
    ///     The "GenerationClient" property provides access to an object that implements the <see cref="IGenerationClient" />
    ///     interface.
    ///     This interface contains methods to generate chat responses based on the provided request object.
    /// </remarks>
    /// <seealso cref="IGenerationClient" />
    IGenerationClient GenerationClient { get; }

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
    ///     Represents the name of the location the player was in BEFORE this location.
    ///     Will be null at the beginning of the game.
    /// </summary>
    string? PreviousLocationName { get; }

    /// <summary>
    /// If, during the previous turn, the adventurer moved from one location to another,
    /// this property represents the direction they travelled to reach the new location.
    /// Will be null at the beginning of the game, or if the player's previous command
    /// kept them in the same location. 
    /// </summary>
    Direction? LastMovementDirection { get; }

    /// <summary>
    ///     Gets the description of the current location in the game.
    /// </summary>
    /// <value>
    ///     The description of the current location in the game.
    /// </value>
    /// <remarks>
    ///     The "LocationDescription" property represents the descriptive text that provides information about the current
    ///     location
    ///     in the game. This text can include details about the surroundings, objects in the location, and any other relevant
    ///     information.
    ///     It is used to give players a visual representation of the game world and guide them in making decisions and
    ///     interacting with their environment.
    /// </remarks>
    string LocationDescription { get; }

    /// <summary>
    ///     Represents the introductory text of the game.
    /// </summary>
    /// <remarks>
    ///     The "IntroText" property contains the initial text that is displayed to the player at the start of the game.
    ///     It provides information about the game's setting, story, and objectives.
    /// </remarks>
    string IntroText { get; }

    /// <summary>
    ///     Indicates the runtime container....is it Web, Console, other?
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

    /// <summary>
    ///     Call this before you try to use the engine.
    /// </summary>
    /// <returns></returns>
    Task InitializeEngine();
}