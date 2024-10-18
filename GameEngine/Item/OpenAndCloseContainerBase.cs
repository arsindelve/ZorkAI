using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace GameEngine.Item;

/// <summary>
///     Represents a base class for containers that can be opened and closed.
/// </summary>
public abstract class OpenAndCloseContainerBase : ContainerBase, IOpenAndClose
{
    public bool HasEverBeenOpened { get; set; }

    public virtual string? CannotBeOpenedDescription(IContext context)
    {
        return null;
    }

    public virtual string AlreadyClosed => "It is already closed.";

    public virtual string AlreadyOpen => "It is already open.";

    public virtual string NowClosed(ILocation currentLocation) => "Closed.";

    public bool IsOpen { get; set; }

    public virtual string NowOpen(ILocation currentLocation) => "Opened";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        InteractionResult? result = null;

        // See if one of the items inside me has a matching interaction.
        if (IsOpen || IsTransparent)
            foreach (var item in Items.ToList())
            {
                result = item.RespondToSimpleInteraction(action, context, client);
                if (result is { InteractionHappened: true })
                    return result;
            }

        if (result != null && result is not NoNounMatchInteractionResult)
            return result;

        if (!action.MatchNoun(NounsForMatching))
            return new NoNounMatchInteractionResult();

        return ApplyProcessors(action, context, null, client);
    }

    public override (bool HasItem, IItem? TheItem) HasMatchingNoun(string? noun, bool lookInsideContainers = true)
    {
        if (IsOpen)
            return base.HasMatchingNoun(noun, lookInsideContainers);

        var hasMatch = NounsForMatching.Any(s => s.Equals(noun, StringComparison.InvariantCultureIgnoreCase));
        return hasMatch ? (true, this) : (false, null);
    }
}