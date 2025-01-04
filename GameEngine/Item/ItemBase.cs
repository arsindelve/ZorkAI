using GameEngine.Item.ItemProcessor;
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
    /// <param name="indent"></param>
    /// <returns></returns>
    public virtual string GenericDescription(ILocation? currentLocation)
    {
        return string.Empty;
    }

    public virtual string? CannotBeTakenDescription { get; set; }

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
    public ICanHoldItems? CurrentLocation { get; set; }

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
    /// <returns>The interaction result.</returns>
    public virtual InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (!action.MatchNounAndAdjective(NounsForMatching))
            return new NoNounMatchInteractionResult();

        return ApplyProcessors(action, context, null, client);
    }

    public virtual int Size => 1;

    public virtual (bool HasItem, IItem? TheItem) HasMatchingNoun(string? noun, bool lookInsideContainers = true)
    {
        var hasItem = NounsForMatching.Any(s => s.Equals(noun, StringComparison.InvariantCultureIgnoreCase));
        if (hasItem)
            return (true, this);

        return (false, null);
    }

    public virtual InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        return new NoNounMatchInteractionResult();
    }

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

    public virtual string OnOpening(IContext context)
    {
        return "";
    }

    private static List<IVerbProcessor> GetProcessors(ItemBase item)
    {
        List<IVerbProcessor> result =
        [
            // anything can be examined
            new ExamineInteractionProcessor(),
            // and smelled 
            new SmellInteractionProcessor()
        ];

        if (item is ICanBeTakenAndDropped)
            result.Add(new TakeOrDropInteractionProcessor());
        else
            result.Add(new CannotBeTakenProcessor());

        if (item is ICanBeTakenAndDropped)
            result.Add(new TakeOrDropInteractionProcessor());

        if (item is ICanBeRead)
            result.Add(new ReadInteractionProcessor());

        if (item is ITurnOffAndOn)
            result.Add(new TurnOnOrOffProcessor());

        if (item is ICannotBeTurnedOff)
            result.Add(new TurnOnOrOffProcessor());

        if (item is IOpenAndClose)
            result.Add(new OpenAndCloseInteractionProcessor());

        if (item is ICanBeEaten)
            result.Add(new EatAndDrinkInteractionProcessor());

        if (item is IAmADrink)
            result.Add(new EatAndDrinkInteractionProcessor());

        if (item is IAmClothing)
            result.Add(new ClothingOnAndOffProcessor());

        return result;
    }

    protected InteractionResult ApplyProcessors(SimpleIntent action, IContext context, InteractionResult? result,
        IGenerationClient client)
    {
        result ??= GetProcessors(this).Aggregate<IVerbProcessor, InteractionResult?>(null, (current, processor)
            => current ?? processor.Process(action, context, this, client));

        return result ?? new NoVerbMatchInteractionResult { Verb = action.Verb, Noun = action.Noun };
    }

    public virtual string? OnBeingTaken(IContext context)
    {
        return null;
    }

    public virtual void OnFailingToBeTaken(IContext context)
    {
    }

    public virtual void OnBeingExamined(IContext context)
    {
    }

    public virtual string? CannotBeClosedDescription(IContext context)
    {
        return null;
    }
}