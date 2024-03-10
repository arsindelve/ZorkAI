using Model.Item;

namespace Game.Item;

/// <summary>
///     Represents a base class for containers that can be opened and closed.
/// </summary>
public class OpenAndCloseContainerBase : ContainerBase, IOpenAndClose
{
    public bool AmIOpen => ((IOpenAndClose)this).IsOpen;

    public bool HasEverBeenOpened { get; set; }

    public virtual string AlreadyClosed => "It is already closed.";

    public virtual string AlreadyOpen => "It is already open.";

    public virtual string NowClosed => "Closed.";

    public bool IsOpen { get; set; }
    
    public virtual string NowOpen => "Opened";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context)
    {
        InteractionResult? result = null;

        if (AmIOpen)
            result = Items
                .ToList()
                .Aggregate(result,
                    (current, item) => current ?? item.RespondToSimpleInteraction(action, context));

        if (result != null && result is not NoNounMatchInteractionResult)
            return result;

        if (!action.MatchNoun(NounsForMatching))
            return new NoNounMatchInteractionResult();

        return ApplyProcessors(action, context, null);
    }

    public override bool HasMatchingNoun(string? noun)
    {
        return AmIOpen
            ? base.HasMatchingNoun(noun)
            : NounsForMatching.Any(s => s.Equals(noun, StringComparison.InvariantCultureIgnoreCase));
    }
}