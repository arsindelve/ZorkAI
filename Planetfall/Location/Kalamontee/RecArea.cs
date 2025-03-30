using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

internal class RecArea : LocationBase
{
    public override string Name => "Rec Area";

    private ConferenceRoomDoor Door => Repository.GetItem<ConferenceRoomDoor>();

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<PlainHall>() },
            { Direction.E, Go<RecCorridor>() },
            {
                Direction.N, new MovementParameters
                {
                    CanGo = _ => Door.IsOpen,
                    Location = GetLocation<ConferenceRoom>(),
                    CustomFailureMessage = "The door is closed. "
                }
            }
        };
    }

    public override void Init()
    {
        StartWithItem<ConferenceRoomDoor>();
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        string[] verbs = ["examine", "look at"];

        if (!action.MatchVerb(verbs))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        if (action.MatchNoun(["games"]))
            return new PositiveInteractionResult(
                "All the usual games -- Chess, Cribbage, Galactic Overlord, Double Fannucci...");

        if (action.MatchNoun(["tapes"]))
            return new PositiveInteractionResult(
                "Let's see...here are some musical selections, here are some bestselling romantic novels, " +
                "here is a biography of a famous Double Fannucci champion...");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a recreational facility of some sort. Games and tapes are scattered about the room. " +
               $"Hallways head off to the east and south, and to the north is a door which is {(Door.IsOpen ? "open" : "closed and locked")}. " +
               $"A dial on the door is currently set to {Door.Code}. ";
    }
}