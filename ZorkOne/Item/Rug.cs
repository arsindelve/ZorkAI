using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Rug : ItemBase
{
    public bool HasBeenMovedAside { get; set; }

    public override string CannotBeTakenDescription => HasBeenMovedAside
        ? "The rug is extremely heavy and cannot be carried."
        : "The rug is too heavy to lift, but in trying to take it you have noticed an irregularity beneath it.";

    public override string[] NounsForMatching => ["rug", "carpet"];

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchNoun(NounsForMatching))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        if (action.MatchVerb(["move", "slide", "pull", "rearrange"]))
            return MoveRug();

        if (action.MatchVerb(["sit"]) && action.OriginalInput != null &&
            action.OriginalInput.ToLowerInvariant().Contains("on"))
            return SitOnRug();

        if (action.MatchVerb(["look", "examine", "peek"]) && action.OriginalInput != null &&
            action.OriginalInput.ToLowerInvariant().Contains("under"))
            return LookUnderRug();
        
        if (action.MatchVerb(["lift", "hoist", "raise"]) && action.OriginalInput != null)
            return LookUnderRug();

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    private InteractionResult LookUnderRug()
    {
        if (HasBeenMovedAside)
            return new PositiveInteractionResult(
                "There is nothing but dust there. ");

        return new PositiveInteractionResult(
            "Underneath the rug is a closed trap door. As you drop the corner of the rug, " +
            "the trap door is once again concealed from view. ");
    }

    private InteractionResult SitOnRug()
    {
        if (HasBeenMovedAside)
            return new PositiveInteractionResult(
                " I suppose you think it's a magic carpet? ");

        return new PositiveInteractionResult(
            "As you sit, you notice an irregularity underneath it. Rather than be uncomfortable, you stand up again. ");
    }

    private InteractionResult MoveRug()
    {
        if (HasBeenMovedAside)
            return new PositiveInteractionResult(
                "Having moved the carpet previously, you find it impossible to move it again. ");

        HasBeenMovedAside = true;
        Repository.GetLocation<LivingRoom>().ItemPlacedHere(Repository.GetItem<TrapDoor>());
        return new PositiveInteractionResult(
            "With a great effort, the rug is moved to one side of the room. With the rug moved, " +
            "the dusty cover of a closed trap door appears. ");
    }
}