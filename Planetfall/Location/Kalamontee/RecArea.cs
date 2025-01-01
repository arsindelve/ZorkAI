using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

internal class RecArea : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.S, Go<PlainHall>() },
            { Direction.E, Go<RecCorridor>() }
        };

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client)
    {
        string[] verbs = ["examine", "look at"];

        if(!action.MatchVerb(verbs))
            return base.RespondToSimpleInteraction(action, context, client);

        if (action.MatchNoun(["games"]))
            return new PositiveInteractionResult("All the usual games -- Chess, Cribbage, Galactic Overlord, Double Fannucci...");

        if (action.MatchNoun(["tapes"]))
            return new PositiveInteractionResult(
                "Let's see...here are some musical selections, here are some bestselling romantic novels, here is a biography of a famous Double Fannucci champion...");

        return base.RespondToSimpleInteraction(action, context, client);
    }

    protected override string GetContextBasedDescription(IContext context) =>
        "This is a recreational facility of some sort. Games and tapes are scattered about the room. " +
        "Hallways head off to the east and south, and to the north is a door which is closed and locked. " +
        "A dial on the door is currently set to 0. ";


    public override string Name => "Rec Area";
}