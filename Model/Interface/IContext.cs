using System.Text.Json.Serialization;
using Model.AIGeneration;
using Model.Intent;
using Model.Interaction;
using Model.Item;
using Model.Location;
using Model.Movement;
using Utilities;

namespace Model.Interface;

/// <summary>
///     The "context" is anything we need to know about the state of the game that is not
///     location or item state dependant. These include the score, number of moves, adventurer inventory...
///     stuff like that. This, along with the state of all objects and locations, (stored in the <see cref="Repository") />
///     encompasses everything we need to know to save and restore the game...i.e preserve the entire game state
/// </summary>
public interface IContext : ICanContainItems
{
    /// <summary>
    /// Retrieves items of the specified type from the adventurer's inventory.
    /// </summary>
    /// <typeparam name="T">The type of items to retrieve, where T implements IItem and has a parameterless constructor.</typeparam>
    /// <returns>A collection of items of the specified type.</returns>
    List<T> GetItems<T>();

    /// <summary>
    /// Sometimes, a major event in the game will change the circumstances so drastically that we need to update the
    /// system prompt with this new information. For example, after the explosion of the Feinstein in Planetfall, we no
    /// longer want the AI to think the player is in space. 
    /// </summary>
    string SystemPromptAddendum { get; set; } 
    
    /// <summary>
    ///     Represents adventurer's current score.
    /// </summary>
    int Score { get; }

    public LimitedStack<string> Inputs { get; set; }

    [JsonIgnore] IGameEngine? Engine { get; set; }

    /// <summary>
    ///     Gets or sets the last noun in the game context.
    /// </summary>
    /// <remarks>
    ///     This property represents the last mentioned noun in the game context.
    ///     It is commonly used to refer to the previous mentioned object.
    /// </remarks>
    string LastNoun { get; set; }

    /// <summary>
    ///     Gets or sets the current location in the game context.
    /// </summary>
    /// <remarks>
    ///     The current location represents the player's current position
    ///     in the game world.
    /// </remarks>
    ILocation CurrentLocation { get; set; }

    /// <summary>
    ///     Property that indicates whether the context has a light source.
    /// </summary>
    /// <returns>True if the context has a light source, false otherwise.</returns>
    /// <remarks>
    ///     This property checks if the context has any items that implement the <see cref="Model.Item.IAmALightSource" />
    ///     interface and are turned on.
    /// </remarks>
    bool HasLightSource { get; }

    /// <summary>
    ///     Gets the number of moves made by the adventurer.
    /// </summary>
    int Moves { get; set; }

    /// <summary>
    ///     A reference to the "game", which can tell us constant, game specific
    ///     things like how to calculate score, starting location, etc.
    /// </summary>
    IInfocomGame Game { get; set; }

    /// <summary>
    ///     Gets or sets the name of the last saved game.
    /// </summary>
    /// <value>The name of the last saved game.</value>
    string? LastSaveGameName { get; set; }

    bool ItIsDarkHere { get; }

    /// <summary>
    ///     Represents a collection of currently-active turn-based actors in the game.
    /// </summary>
    public List<ITurnBasedActor> Actors { get; set; }

    /// <summary>
    ///     What is the total weight of everything we're carrying?
    /// </summary>
    int CarryingWeight { get; }

    /// <summary>
    ///     Gets or sets the verbosity - how descriptive are we when we enter
    ///     a new location?
    /// </summary>
    Verbosity Verbosity { get; set; }

    /// <summary>
    ///     Represents the current score of the adventurer.
    /// </summary>
    /// <remarks>
    ///     The score is used to keep track of the player's progress and achievements in the game.
    ///     It is an integer value and can be accessed through the "CurrentScore" property.
    /// </remarks>
    string CurrentScore { get; }

    /// <summary>
    ///     If the adventurer has moved from one location to another in the last turn, in which
    ///     direction did they travel to get there?
    /// </summary>
    Direction? LastMovementDirection { get; set; }

    /// <summary>
    ///     For debugging purposes, will list everything in inventory.
    /// </summary>
    /// <returns></returns>
    public string LogItems();

    void Take(IItem item);

    /// <summary>
    ///     Remove the item from the adventurer's inventory and put it in the current location.
    /// </summary>
    /// <param name="item"></param>
    void Drop(IItem item);

    /// <summary>
    ///     Remove the item from the adventurer's inventory and put it in the current location.
    /// </summary>
    void Drop<T>() where T : IItem, new();

    /// <summary>
    ///     Adds the specified number of points to the score.
    /// </summary>
    /// <param name="points">The number of points to add.</param>
    /// <returns>The updated score after adding the points.</returns>
    int AddPoints(int points);

    /// <summary>
    /// Processes a simple interaction intent within the context and generates an appropriate response.
    /// </summary>
    /// <param name="simpleInteraction">The simple interaction intent to be processed.</param>
    /// <param name="client">The client used for generation-related operations.</param>
    /// <param name="itemProcessorFactory">The factory used to process items associated with the interaction.</param>
    /// <returns>An <see cref="InteractionResult"/> containing the outcome of the interaction processing.</returns>
    Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent simpleInteraction, IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory);

    /// <summary>
    /// Generates a description of the items located in a specific area within the game's environment.
    /// </summary>
    /// <param name="locationName">The name of the location where the items are being described.</param>
    /// <param name="location">The location object representing the area where the items are to be described. Can be null if no specific location is provided.</param>
    /// <returns>A string containing a detailed description of the items in the specified location.</returns>
    string ItemListDescription(string locationName, ILocation? location);

    /// <summary>
    ///     Does the context need to do any processing at the beginning of the turn?
    /// </summary>
    /// <returns>Text to prepend to the response, if any. </returns>
    string? ProcessBeginningOfTurn();

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

    /// <summary>
    ///     Removes an actor from the game engine. Once removed,
    ///     the actor will no longer act every turn.
    /// </summary>
    void RemoveActor<TActor>() where TActor : ITurnBasedActor;

    /// <summary>
    ///     See if the context needs to do any processing at the end of the turn. Append the output, if any
    /// </summary>
    /// <returns></returns>
    string? ProcessEndOfTurn();

    /// <summary>
    ///     Allows game-specific contexts to provide a custom save game request.
    ///     For example, Planetfall can return a Floyd-specific request when Floyd is present.
    /// </summary>
    /// <param name="location">The current location description.</param>
    /// <returns>A custom Request for save game narration, or null to use the default AfterSaveGameRequest.</returns>
    Model.AIGeneration.Request? GetSaveGameRequest(string location);
}