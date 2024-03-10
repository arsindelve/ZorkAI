using Model.Intent;
using Model.Interaction;
using Model.Item;

namespace Model;

public interface ILocation
{
    string Description { get; }

    string DescriptionForGeneration { get; }

    /// <summary>
    ///     Gets called when we enter a location.
    /// </summary>
    void OnEnterLocation(IContext context);

    /// <summary>
    ///     We have parsed the user input and determined that we have a <see cref="SimpleIntent" /> corresponding
    ///     of a verb and a noun. Does that combination do anything in this location? The default implementation
    ///     of the base class checks each item in this locations and asks them if they provide any interaction. This
    ///     method will be frequently overriden in child locations.
    /// </summary>
    /// <param name="action">The action to examine. Can we do anything with it?</param>
    /// <param name="context">The current context, in case we need it during action processing.</param>
    /// <returns>InteractionResult that describes if and and how the interaction took place.</returns>
    InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context);

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
    /// <returns>True if the location has an item that matches the given noun; otherwise, false.</returns>
    bool HasMatchingNoun(string? noun);
}