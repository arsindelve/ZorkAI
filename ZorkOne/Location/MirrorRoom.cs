using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Location;

internal abstract class MirrorRoom : LocationBase, IThiefMayVisit
{
    public override string Name => "Mirror Room";

    protected override string GetContextBasedDescription(IContext context)
    {
        if (GetItem<Mirror>().IsBroken)
            return "You are in a large square room with tall ceilings. The enormous mirror which once filled " +
                   "the south wall now lies shattered in countless pieces. There are exits on the other three " +
                   "sides of the room.";

        return "You are in a large square room with tall ceilings. On the south wall is an " +
               "enormous mirror which fills the entire wall. There are exits on the other three sides of the room.";
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchNoun(["mirror"]))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        var mirror = GetItem<Mirror>();

        // Breaking the mirror (mung/throw/attack in the original ZIL MIRROR-MIRROR) permanently disables
        // the teleport. Reuse the shared attack/throw synonym sets so the verb coverage stays consistent
        // with the rest of the engine, plus the break-specific verbs the original recognized.
        if (action.MatchVerb([..Verbs.KillVerbs, ..Verbs.ThrowVerbs, ..Verbs.BreakVerbs, "kick", "hit"]))
        {
            if (mirror.IsBroken)
                return new PositiveInteractionResult("Haven't you done enough damage already? ");

            mirror.IsBroken = true;
            return new PositiveInteractionResult(
                "You have broken the mirror. I hope you have a seven years' supply of good luck handy. ");
        }

        if (action.MatchVerb(["look", "examine", "peer"]))
            return new PositiveInteractionResult(mirror.IsBroken
                ? Mirror.BrokenDescription
                : "There is an ugly person staring back at you. ");

        if (action.MatchVerb(["rub", "touch", "feel", "press"]))
        {
            // A broken mirror no longer teleports the player between the two mirror rooms.
            if (mirror.IsBroken)
                return new PositiveInteractionResult(Mirror.BrokenDescription);

            if (this is MirrorRoomNorth)
                context.CurrentLocation = GetLocation<MirrorRoomSouth>();
            else
                context.CurrentLocation = GetLocation<MirrorRoomNorth>();

            return new PositiveInteractionResult("There is a rumble from deep within the earth and the room shakes. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}