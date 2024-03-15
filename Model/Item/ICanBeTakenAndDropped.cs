namespace Model.Item;

/// <summary>
///     Represents an item that can be taken and dropped. This would include swords and leaflets, but not
///     items like windows and mailboxes.
/// </summary>
public interface ICanBeTakenAndDropped : IInteractionTarget
{
    /// <summary>
    ///     Represents the description of an item when it is on the ground.
    /// </summary>
    /// <remarks>
    ///     This property should provide a description of the item when it is lying on the ground in the game world.
    /// </remarks>
    string OnTheGroundDescription { get; }

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
    string? NeverPickedUpDescription { get; }

    /// <summary>
    ///     Indicates whether the item has ever been picked up.
    /// </summary>
    /// <seealso cref="ICanBeTakenAndDropped" />
    bool HasEverBeenPickedUp { get; }

    /// <summary>
    /// This method is called when the item is being taken by the player.
    /// </summary>
    /// <param name="context">The context of the game.</param>
    void OnBeingTaken(IContext context);
}