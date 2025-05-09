using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Command;

namespace Planetfall.Location.Kalamontee.Admin;

internal abstract class RiftLocationBase : LocationWithNoStartingItems
{
    internal static readonly string[] RiftNouns = ["rift", "gap", "split", "crack", "fissure", "break", "chasm", "gaping rift"];

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        if (!action.MatchVerb(Verbs.ThrowVerbs) || !action.MatchNounTwo(RiftNouns))
            return await base.RespondToMultiNounInteraction(action, context);

        // What are we throwing? 
        var itemWeJustLostForever = Repository.GetItem(action.NounOne);
        if (itemWeJustLostForever is null)
            return await base.RespondToMultiNounInteraction(action, context);

        itemWeJustLostForever.CurrentLocation?.RemoveItem(itemWeJustLostForever);
        itemWeJustLostForever.CurrentLocation = null;
        return new PositiveInteractionResult(
            $"The {itemWeJustLostForever.NounsForMatching.OrderByDescending(s => s.Length).First()} sails gracefully into the rift. ");
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["jump", "leap"], RiftNouns))
            return new DeathProcessor().Process(
                "You get a brief (but much closer) view of the sharp and nasty rocks at the bottom of the rift. ",
                context);

        if (action.Match(["examine", "look at", "look into"], RiftNouns))
            return new PositiveInteractionResult(
                "The rift is at least eight meters wide and more than thirty meters deep. The bottom is covered with sharp and nasty rocks. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}