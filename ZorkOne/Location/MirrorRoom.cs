using Model.AIGeneration;
using Model.Intent;

namespace ZorkOne.Location;

public abstract class MirrorRoom : LocationWithNoStartingItems
{
    protected override string ContextBasedDescription =>
        "You are in a large square room with tall ceilings. On the south wall is an " +
        "enormous mirror which fills the entire wall. There are exits on the other three sides of the room.";

    public override string Name => "Mirror Room";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (!action.MatchNoun(["mirror"]))
            return base.RespondToSimpleInteraction(action, context, client);

        if (action.MatchNoun(["look", "examine", "peer"]))
            return new PositiveInteractionResult("There is an ugly person staring back at you. ");

        if (action.MatchVerb(["rub", "touch", "feel", "press"]))
        {
            if (this is MirrorRoomNorth)
                context.CurrentLocation = GetLocation<MirrorRoomSouth>();
            else
                context.CurrentLocation = GetLocation<MirrorRoomNorth>();
            
            return new PositiveInteractionResult("There is a rumble from deep within the earth and the room shakes. ");
        }

        return base.RespondToSimpleInteraction(action, context, client);
    }
}