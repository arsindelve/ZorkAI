using Model.Item;

namespace Game.Item;

/// <summary>
///     The base class for all items in the game.
/// </summary>
public abstract class ItemBase : IItem
{
    public virtual string InInventoryDescription => "";

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
    public virtual string NeverPickedUpDescription => InInventoryDescription;

    public virtual string? CannotBeTakenDescription { get; set; }

    public bool HasEverBeenPickedUp { get; set; }

    /// <summary>
    ///     When checking if an item matches the noun of a <see cref="SimpleIntent" />, this is the
    ///     property to check.
    /// </summary>
    public virtual string[] NounsForMatching { get; } = [];

    /// <summary>
    ///     Gets or sets the current location of the item. Is it in inventory? In the mailbox?
    ///     Beside the house?
    /// </summary>
    /// <value>
    ///     The current location of the item.
    /// </value>
    public ICanHoldItems? CurrentLocation { get; set; }

    /// <summary>
    ///     Responds to a simple interaction based on the given action and context. Does the user's intent
    ///     "noun" match our <see cref="NounsForMatching" />? If so, does the user's intent have any effect on this
    ///     item at all?
    /// </summary>
    /// <param name="action">The simple intent representing the action.</param>
    /// <param name="context">The context in which the interaction takes place.</param>
    /// <returns>The interaction result.</returns>
    public virtual InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context)
    {
        if (!action.MatchNoun(NounsForMatching))
            return new NoNounMatchInteractionResult();

        return ApplyProcessors(action, context, null);
    }

    protected static List<IVerbProcessor> GetProcessors(ItemBase item)
    {
        List<IVerbProcessor> result = new();

        if (item is ICanBeExamined)
            result.Add(new ExamineInteractionProcessor());

        if (item is ICanBeTakenAndDropped)
            result.Add(new TakeOrDropInteractionProcessor());
        else
            result.Add(new CannotBeTakenProcessor());

        if (item is ICanBeTakenAndDropped)
            result.Add(new TakeOrDropInteractionProcessor());

        if (item is ICanBeRead)
            result.Add(new ReadInteractionProcessor());

        if (item is ICanBeTurnedOnAndOff)
            result.Add(new TurnOnOrOffProcessor());

        if (item is IOpenAndClose)
            result.Add(new OpenAndCloseInteractionProcessor());

        if (item is ICanBeEaten)
            result.Add(new EatInteractionProcessor());

        return result;
    }

    protected InteractionResult ApplyProcessors(SimpleIntent action, IContext context, InteractionResult? result)
    {
        result ??= GetProcessors(this).Aggregate<IVerbProcessor, InteractionResult?>(null, (current, processor)
            => current ?? processor.Process(action, context, (IInteractionTarget)this));

        return result ?? new NoVerbMatchInteractionResult { Verb = action.Verb, Noun = action.Noun };
    }

    public virtual bool HasMatchingNoun(string? noun)
    {
        return NounsForMatching.Any(s => s.Equals(noun, StringComparison.InvariantCultureIgnoreCase));
    }
}