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
public interface IContext : ICanHoldItems
{
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

    InteractionResult RespondToSimpleInteraction(SimpleIntent simpleInteraction, IGenerationClient client);

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
    ///     See if the context needs to do any processing at the end of the turn. Append the output, if any
    /// </summary>
    /// <returns></returns>
    string? ProcessEndOfTurn();

}