using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Location;

internal abstract class MirrorRoom : LocationBase, IThiefMayVisit
{
    // Breaking the mirror permanently disables the touch-to-teleport. Reuse the shared attack and break
    // synonym sets so the verb coverage stays consistent with the rest of the engine, plus "kick". Throw is
    // intentionally excluded: "throw the mirror" makes no sense for a wall-sized fixture. Built once, not per call.
    private static readonly string[] BreakMirrorVerbs =
        [..Verbs.KillVerbs, ..Verbs.BreakVerbs, "kick"];

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

        if (action.MatchVerb(BreakMirrorVerbs))
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

        if (action.MatchVerb(Verbs.TouchVerbs))
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

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        // "throw <X> at mirror": anything you hurl at the mirror shatters it (and disables the teleport),
        // except a few soft, lightweight items that just bounce off. Either way the item ends up on the floor.
        if (!action.MatchVerb(Verbs.ThrowVerbs) || !action.MatchNounTwo(["mirror"]))
            return await base.RespondToMultiNounInteraction(action, context);

        var (hasItem, thrownItem) = context.HasMatchingNoun(action.NounOne);
        if (!hasItem || thrownItem is null)
            return new PositiveInteractionResult("You don't have that. ");

        var mirror = GetItem<Mirror>();
        var itemName = thrownItem.NounsForMatching.OrderByDescending(s => s.Length).First();

        // The item leaves your hands and lands on the floor of this room, broken mirror or not.
        ItemPlacedHere(thrownItem);

        // Soft, lightweight items aren't going to shatter a wall-sized mirror.
        if (thrownItem is Garlic or BrownSack or Lunch)
            return new PositiveInteractionResult(
                $"The {itemName} bounces off the mirror and falls harmlessly to the floor. ");

        if (mirror.IsBroken)
            return new PositiveInteractionResult(
                $"Haven't you done enough damage already? The {itemName} falls to the floor. ");

        mirror.IsBroken = true;
        return new PositiveInteractionResult(
            $"The {itemName} shatters the mirror and falls to the floor among the fragments. " +
            "I hope you have a seven years' supply of good luck handy. ");
    }
}