using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace GameEngine.Item;

/// <summary>
///     Represents a base class for containers that can be opened and closed.
/// </summary>
public abstract class OpenAndCloseContainerBase : ContainerBase, IOpenAndClose, ICanBeExamined
{
    public bool HasEverBeenOpened { get; set; }

    /// <summary>
    ///     Default examine behavior for an open/close container: when closed, report it is closed;
    ///     when open, list the contents (or that it is empty). This closes the recurring gap
    ///     (issue #398) where a subclass hand-wrote an <c>ExaminationDescription</c> that reported
    ///     only "is open" and hid what was inside.
    ///     <para>
    ///     Implemented as an <b>explicit</b> interface member on purpose: subclasses that need
    ///     special examine text simply re-list <see cref="ICanBeExamined" /> and declare their own
    ///     public <c>ExaminationDescription</c> - that re-implementation wins for their type with no
    ///     member-hiding warning. Subclasses that want the correct default just leave it off and
    ///     inherit this.
    ///     </para>
    /// </summary>
    string ICanBeExamined.ExaminationDescription =>
        IsOpen ? ItemListDescription(Name, null) : $"The {Name} is closed. ";

    public virtual string? CannotBeOpenedDescription(IContext context)
    {
        return null;
    }

    public virtual string AlreadyClosed => "It is already closed.";

    public virtual string AlreadyOpen => "It is already open.";

    public virtual string NowClosed(ILocation currentLocation)
    {
        return "Closed.";
    }

    public virtual bool IsOpen { get; set; }

    public virtual string NowOpen(ILocation currentLocation)
    {
        return "Opened.";
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        InteractionResult? result = null;

        // See if one of the items inside me has a matching interaction.
        if (IsOpen || IsTransparent)
            foreach (var item in Items.ToList())
            {
                result = await item.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
                if (result is { InteractionHappened: true })
                    return result;
            }

        if (result != null && result is not NoNounMatchInteractionResult)
            return result;

        // If this container is open or transparent, make sure all the nouns of the 
        // items inside have processors applied. 
        var nounsForProcessing = NounsForMatching.ToList();
        if (IsOpen || IsTransparent)
            nounsForProcessing.AddRange(Items.SelectMany(s => s.NounsForMatching));

        if (!action.MatchNoun(nounsForProcessing.ToArray()))
            return new NoNounMatchInteractionResult();

        return await ApplyProcessors(action, context, null, client, itemProcessorFactory);
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        // A closed, non-transparent container hides its contents from multi-noun interactions, just like simple ones.
        if (!IsOpen && !IsTransparent)
            return new NoNounMatchInteractionResult();

        return await base.RespondToMultiNounInteraction(action, context);
    }

    public override (bool HasItem, IItem? TheItem) HasMatchingNoun(string? noun, bool lookInsideContainers = true)
    {
        // If open or transparent, search inside
        if (IsOpen || IsTransparent)
            return base.HasMatchingNoun(noun, lookInsideContainers);

        // If closed and not transparent, only match the container itself
        var hasMatch = NounsForMatching.Any(s => s.Equals(noun, StringComparison.InvariantCultureIgnoreCase));
        return hasMatch ? (true, this) : (false, null);
    }
}