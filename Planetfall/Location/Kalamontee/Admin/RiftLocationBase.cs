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
        if (action.Match(Verbs.JumpVerbs, RiftNouns))
            return new DeathProcessor().Process(
                "You get a brief (but much closer) view of the sharp and nasty rocks at the bottom of the rift. ",
                context);

        if (action.Match(Verbs.ExamineVerbs, RiftNouns))
            return new PositiveInteractionResult(
                "The rift is at least eight meters wide and more than thirty meters deep. The bottom is covered with sharp and nasty rocks. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    // Issue #369: "jump rift" / "jump into rift" / "jump over rift" never reached the death branch
    // above. The AI complex-intent parser reads the rift as "the thing to the north" and misclassifies
    // these as a MoveIntent toward Direction.N, which only ever produces the mundane "too wide to jump
    // across" movement-failure message from AdminCorridor/AdminCorridorNorth's Map(). Catching the raw
    // input here - RespondToSpecificLocationInteraction runs before any parser (deterministic or AI)
    // gets a chance to claim it - lets the already-correct death scene above actually fire.
    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input,
        IContext context, IGenerationClient client)
    {
        if (IsJumpingIntoTheRift(input))
            return new DeathProcessor().Process(
                "You get a brief (but much closer) view of the sharp and nasty rocks at the bottom of the rift. ",
                context);

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }

    private static bool IsJumpingIntoTheRift(string? input)
    {
        var words = input?.ToLowerInvariant().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (words is null || words.Count == 0 || !Verbs.JumpVerbs.Contains(words[0]))
            return false;

        words.RemoveAt(0);

        // Drop the filler prepositions/articles a player might naturally insert - "jump into the
        // rift" and "jump over rift" mean the same thing as the bare "jump rift".
        string[] filler = ["into", "in", "over", "across", "through", "the"];
        words.RemoveAll(filler.Contains);

        return RiftNouns.Contains(string.Join(' ', words));
    }
}