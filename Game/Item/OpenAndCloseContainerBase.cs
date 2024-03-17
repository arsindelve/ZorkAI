using Model.Item;

namespace Game.Item;

/// <summary>
///     Represents a base class for containers that can be opened and closed.
/// </summary>
public abstract class OpenAndCloseContainerBase : ContainerBase, IOpenAndClose
{
    public bool HasEverBeenOpened { get; set; }

    public virtual string AlreadyClosed => "It is already closed.";

    public virtual string AlreadyOpen => "It is already open.";

    public virtual string NowClosed => "Closed.";

    public bool IsOpen { get; set; }

    public virtual string NowOpen => "Opened";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context)
    {
        InteractionResult? result = null;

        // See if one of the items inside me has a matching interaction.
        if (IsOpen || IsTransparent)
            foreach (var item in Items.ToList())
            {
               result = item.RespondToSimpleInteraction(action, context);
               if (result is { InteractionHappened: true }) 
                   return result;
            }

        if (result != null && result is not NoNounMatchInteractionResult)
            return result;

        if (!action.MatchNoun(NounsForMatching))
            return new NoNounMatchInteractionResult();

        return ApplyProcessors(action, context, null);
    }

    public override bool HasMatchingNoun(string? noun, bool lookInsideContainers = true)
    {
        return IsOpen
            ? base.HasMatchingNoun(noun, lookInsideContainers)
            : NounsForMatching.Any(s => s.Equals(noun, StringComparison.InvariantCultureIgnoreCase));
    }
}