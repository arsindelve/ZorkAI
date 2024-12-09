using Model.AIGeneration;
using Model.Intent;
using Model.Interaction;
using Model.Interface;
using Model.Item;
using Model.Movement;

namespace Model.Location;

/// <summary>
/// Represents a location the adventurer can be in. It can hold items, has a name, a description and usually
/// can be moved in and out of using a "Map" that defines the exits. 
/// </summary>
public interface ILocation
{
    string Description { get; }

    /// <summary>
    ///     This allows us to provide a different description of the current location when we use it as
    ///     part of a prompt for AI.
    /// </summary>
    string DescriptionForGeneration { get; }

    string Name { get; }

    /// <summary>
    ///     Gets called the first time a location is referenced.
    /// </summary>
    void Init();

    /// <summary>
    ///     Gets called when we enter a location in case there are any interactions
    ///     we need to process when we walk in the room, BEFORE the description of the room
    /// </summary>
    string BeforeEnterLocation(IContext context, ILocation previousLocation);
    
    /// <summary>
    /// This property represents a sub-location inside another location. It can be used to define a location
    /// that exists within another location, such as a vehicle or a specific area within a larger space.
    /// </summary>
    ISubLocation? SubLocation { get; set; }

    /// <summary>
    /// How many times have we been here before? 
    /// </summary>
    int VisitCount { get; set; }

    /// <summary>
    ///     We have parsed the user input and determined that we have a <see cref="SimpleIntent" /> corresponding
    ///     of a verb and a noun. Does that combination do anything in this location? The default implementation
    ///     of the base class checks each item in this locations and asks them if they provide any interaction. This
    ///     method will be frequently overriden in child locations.
    /// </summary>
    /// <param name="action">The action to examine. Can we do anything with it?</param>
    /// <param name="context">The current context, in case we need it during action processing.</param>
    /// <param name="client"></param>
    /// <returns>InteractionResult that describes if and and how the interaction took place.</returns>
    InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client);

    /// <summary>
    ///     We have parsed the user input and determined that we have a <see cref="MultiNounIntent" /> corresponding
    ///     of a verb and two nouns. Does that combination do anything in this location? The default implementation
    ///     of the base class checks each item in this locations and asks them if they provide any interaction. This
    ///     method will be frequently overriden in child locations.
    /// </summary>
    /// <param name="action">The multi-noun intent representing the interaction.</param>
    /// <param name="context">The context in which the interaction occurs.</param>
    /// <returns>An InteractionResult indicating the result of the interaction.</returns>
    InteractionResult? RespondToMultiNounInteraction(MultiNounIntent action, IContext context);

    /// <summary>
    ///     We're trying to move in a direction. Return the <see cref="MovementParameters" /> object
    ///     that tells us if we can get there, and if so, where we end up. If there is no <see cref="MovementParameters" />
    ///     object corresponding to that location, then we cannot, by default, go that way (indicated by
    ///     a null response)
    /// </summary>
    /// <param name="direction">The direction we want to go from here. </param>
    /// <returns>
    ///     A <see cref="MovementParameters" /> that describes our ability to move there, or null
    ///     if movement in that direction is always impossible
    /// </returns>
    MovementParameters? Navigate(Direction direction);

    /// <summary>
    ///     Checks if the specified item type is present in the current location.
    /// </summary>
    /// <typeparam name="T">The type of item to check for.</typeparam>
    /// <returns>True if the item is present, false otherwise.</returns>
    bool HasItem<T>() where T : IItem, new();

    /// <summary>
    ///     Checks if the location has an item that matches the given noun.
    /// </summary>
    /// <param name="noun">The noun to match against the items in the location.</param>
    /// <param name="lookInsideContainers"></param>
    /// <returns>True if the location has an item that matches the given noun; otherwise, false.</returns>
    (bool HasItem, IItem? TheItem) HasMatchingNoun(string? noun, bool lookInsideContainers = true);

    /// <summary>
    ///     Gets called when we enter a location in case there are any interactions
    ///     we need to process when we walk in the room, AFTER the description of the room
    /// </summary>
    Task<string> AfterEnterLocation(IContext context, ILocation previousLocation, IGenerationClient generationClient);

    /// <summary>
    ///     Gets called when the player leaves the current location and moves to a new location.
    /// </summary>
    /// <param name="context">The current context.</param>
    /// <param name="newLocation"></param>
    /// <param name="previousLocation"></param>
    void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation);

    /// <summary>
    ///     Responds to a (usually) simple, single verb interaction in the location, such as "jump" or "scream". 
    ///     This method is called very early in the response pipeline, even before the input is parsed so the
    ///     input is raw, and locations should only override this if they need to give a very specific response
    ///     to a very specific raw input. 
    /// </summary>
    /// <param name="input">The input string representing the interaction.</param>
    /// <param name="context"></param>
    /// <param name="client">AI generation client</param>
    /// <returns>An object of type InteractionResult that describes the result of the interaction.</returns>
    Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context, IGenerationClient client);

    /// <summary>
    ///     Event handler for a location when the player has waited. Rarely used, but some locations
    ///     will have a special action (waiting in the river causes the boat to go downstream immediately) 
    /// </summary>
    /// <param name="context"></param>
    void OnWaiting(IContext context);

    /// <summary>
    /// Some item got "put" here, or appeared suddenly like when the canary drops the bauble. 
    /// </summary>
    /// <param name="item"></param>
    void ItemPlacedHere(IItem item);

    /// <summary>
    /// For debugging purposes, list all the items in this location. 
    /// </summary>
    /// <returns></returns>
    string LogItems();

}