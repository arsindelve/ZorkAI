using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace GameEngine.Item;

/// <summary>
///     The base class for all items in the game.
/// </summary>
public abstract class ItemBase : IItem
{
    /// <summary>
    ///     If the item can be picked up, this is the description in inventory..."A rope", "A towel".
    ///     If the item cannot be picked up, this is the description of the item, on the ground, where you find it.
    ///     This can be left blank, and then these items will not appear in the location description, but can still be
    ///     interacted with. Sometimes this is done because the item description is part of the room or container description
    ///     like the trap door, or the item is special and just does not show up, like the slime.
    /// </summary>
    /// <param name="currentLocation"></param>
    /// <returns></returns>
    public virtual string GenericDescription(ILocation? currentLocation)
    {
        return string.Empty;
    }

    /// <summary>
    /// Provides a description explaining why the item cannot be taken.
    /// This may vary depending on the state of the object or the context
    /// in which the attempt to take the item occurs.
    /// </summary>
    public virtual string? CannotBeTakenDescription { get; set; }

    /// <summary>
    /// Indicates whether the item has ever been picked up by a player
    /// or interacted with in a manner that would mark it as picked up.
    /// </summary>
    public bool HasEverBeenPickedUp { get; set; }

    /// <summary>
    ///     When checking if an item matches the noun of a <see cref="SimpleIntent" />, this is the
    ///     property to check.
    /// </summary>
    public abstract string[] NounsForMatching { get; }

    /// <summary>
    /// These are almost always identical. When they are not, this can be overriden. 
    /// </summary>
    public virtual string[] NounsForPreciseMatching => NounsForMatching;

    public virtual string Name => NounsForMatching.First();

    /// <summary>
    ///     Gets or sets the current location of the item. Is it in inventory? In the mailbox?
    ///     Beside the house?
    /// </summary>
    /// <value>
    ///     The current location of the item.
    /// </value>
    public ICanContainItems? CurrentLocation { get; set; }

    /// <summary>
    /// Checks if the current item matches the provided noun and adjective combination.
    /// </summary>
    /// <param name="noun">The noun to check for a match.</param>
    /// <param name="adjective">The adjective to check for a match. Can be null or empty.</param>
    /// <param name="lookInsideContainers">Determines if the method should look inside containers for matching items. Defaults to true.</param>
    /// <returns>
    /// A tuple containing a boolean indicating if a matching item was found, and the matching item itself if found.
    /// </returns>
    public (bool HasItem, IItem? TheItem) HasMatchingNounAndAdjective(string? noun, string? adjective,
        bool lookInsideContainers = true)
    {
        if (string.IsNullOrEmpty(adjective))
            return HasMatchingNoun(noun, lookInsideContainers);

        var match = NounsForMatching.Any(s =>
            s.Equals($"{adjective} {noun}", StringComparison.InvariantCultureIgnoreCase));

        if (match)
            return (true, this);

        return (false, null);
    }

    /// <summary>
    ///     Responds to a simple interaction based on the given action and context. Does the user's intent
    ///     "noun" match our <see cref="NounsForMatching" />? If so, does the user's intent have any effect on this
    ///     item at all?
    /// </summary>
    /// <param name="action">The simple intent representing the action.</param>
    /// <param name="context">The context in which the interaction takes place.</param>
    /// <param name="client"></param>
    /// <param name="itemProcessorFactory"></param>
    /// <returns>The interaction result.</returns>
    public virtual async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchNounAndAdjective(NounsForMatching))
            return new NoNounMatchInteractionResult();

        return await ApplyProcessors(action, context, null, client, itemProcessorFactory);
    }

    public virtual int Size => 1;

    /// <summary>
    /// Checks if the current item matches the specified noun.
    /// </summary>
    /// <param name="noun">The noun to match against the item's nouns.</param>
    /// <param name="lookInsideContainers">Indicates whether to look inside containers for a matching item.</param>
    /// <returns>A tuple indicating whether a matching item was found and, if found, the matching item.</returns>
    public virtual (bool HasItem, IItem? TheItem) HasMatchingNoun(string? noun, bool lookInsideContainers = true)
    {
        var hasItem = NounsForMatching.Any(s => s.Equals(noun, StringComparison.InvariantCultureIgnoreCase));
        if (hasItem)
            return (true, this);

        return (false, null);
    }

    /// <summary>
    /// Handles interactions that involve multiple nouns within the game environment. This method implements behavior to process specific multi-noun actions
    /// depending on the current context and the derived class overrides.
    /// </summary>
    /// <param name="action">The multi-noun intent representing the action being taken, including the associated nouns.</param>
    /// <param name="context">The current context in which the interaction takes place, providing relevant state and environment data.</param>
    /// <returns>An <see cref="InteractionResult"/> that represents the outcome of the multi-noun interaction.</returns>
    public virtual Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        return Task.FromResult<InteractionResult?>(new NoNounMatchInteractionResult());
    }

    /// <summary>
    /// Represents a callback action that is triggered when the item is taken.
    /// The action receives the current context in which the item is taken
    /// and can perform any necessary logic or side effects related to the event.
    /// </summary>
    public Action<IContext>? OnBeingTakenCallback { get; set; }

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
    public virtual string NeverPickedUpDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    /// <summary>
    /// Handles the logic to be executed when the item is opened.
    /// This can include describing the opened item's state, triggering related events,
    /// or returning descriptive text for the player.
    /// </summary>
    /// <param name="context">The current game context, providing information such as the player's location, items available, and game state.</param>
    /// <returns>A string describing the result of opening the item, or an empty string if no special behavior is defined.</returns>
    public virtual string OnOpening(IContext context)
    {
        return "";
    }

    protected async Task<InteractionResult> ApplyProcessors(SimpleIntent action, IContext context, InteractionResult? result,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        foreach (var processor in itemProcessorFactory.GetProcessors(this))
        {
            result ??= await processor.Process(action, context, this, client);
            if (result != null)
                break;
        }

        return result ?? new NoVerbMatchInteractionResult { Verb = action.Verb, Noun = action.Noun };
    }

    /// <summary>
    /// Handles the event when an item is being taken from its current location.
    /// This method can provide a description or additional logic to execute when the item is picked up.
    /// It may return a custom response string or defer to the base implementation.
    /// </summary>
    /// <param name="context">The context within which the item is being taken. Provides information
    /// about the current game state, including the player's inventory and environment.</param>
    /// <returns>A string describing the effect or response of taking the item, or null if no specific response is provided.</returns>
    public virtual string? OnBeingTaken(IContext context)
    {
        return null;
    }

    /// <summary>
    /// Invoked when an item fails to be taken by the player. This method allows for custom behavior or messaging
    /// when the specified item cannot be picked up due to certain conditions.
    /// </summary>
    /// <param name="context">
    /// The current game context, containing information about the player's status, location, and game state.
    /// </param>
    public virtual void OnFailingToBeTaken(IContext context)
    {
    }

    /// <summary>
    /// Handles the logic to be executed when the item is being examined in the game context.
    /// </summary>
    /// <param name="context">
    /// The current game context which provides details such as the player's current location, inventory,
    /// and surrounding items or conditions.
    /// </param>
    public virtual void OnBeingExamined(IContext context)
    {
    }

    /// <summary>
    /// Provides a description explaining why certain doors or items cannot be closed in the current context.
    /// </summary>
    /// <param name="context">
    /// The current game state and environment information, used to determine the appropriate description.
    /// </param>
    /// <returns>
    /// A string message describing why the item cannot be closed, or null if no description is provided.
    /// </returns>
    public virtual string? CannotBeClosedDescription(IContext context)
    {
        return null;
    }
}