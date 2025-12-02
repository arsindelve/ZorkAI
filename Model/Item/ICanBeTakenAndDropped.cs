using Model.Interface;
using Model.Location;

namespace Model.Item;

/// <summary>
///     Represents an item that can be taken and dropped. This would include swords and leaflets, but not
///     items like windows and mailboxes - except sometimes the game will allow you to ATTEMPT to pick those up
///     even though it's impossible, in which case you need to implement "CannotBeTakenDescription" which when non
///     empty, will be returned to tell the adventurer that they cannot pick up the item. 
/// </summary>
public interface ICanBeTakenAndDropped : IInteractionTarget
{
    /// <summary>
    ///     Indicates whether the item has ever been picked up.
    /// </summary>
    /// <seealso cref="ICanBeTakenAndDropped" />
    bool HasEverBeenPickedUp { get; }

    /// <summary>
    ///     Represents the description of an item when it is on the ground.
    /// </summary>
    /// <remarks>
    ///     This property should provide a description of the item when it is lying on the ground in the game world.
    ///     "There is a rope here. "
    ///     "Unfortunately, one of those stupid Blow'k-bibben-Gordo brochures is here. "
    /// </remarks>
    string OnTheGroundDescription(ILocation currentLocation);

    /// <summary>
    ///     Gets the description when the item has never been picked up.
    /// </summary>
    /// <value>
    ///     <remarks>
    ///         The sword and lantern, for example, have different descriptions when you first
    ///         see them versus after you drop them somewhere.
    ///     </remarks>
    ///     The never picked up description of the item.
    /// </value>
    string? NeverPickedUpDescription(ILocation currentLocation);

    /// <summary>
    ///     This method is called when the item is being taken by the player.
    /// </summary>
    /// <param name="context">The context of the game.</param>
    /// <param name="previousLocation">The previous holder, if any</param>
    string? OnBeingTaken(IContext context, ICanContainItems? previousLocation);

    /// <summary>
    ///     This method is called when the item fails to be taken by the player. Examples would
    ///     be something that is destroyed by trying to take it, or something that kills you when you
    ///     pick it up. This is different from something which cannot be picked up at all.
    /// </summary>
    /// <param name="context">The context of the game.</param>
    void OnFailingToBeTaken(IContext context);
}