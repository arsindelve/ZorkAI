using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Admin;

internal class PlanRoom : LocationWithNoStartingItems
{
    public override string Name => "Plan Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<AdminCorridorNorth>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a small room whose far wall is covered with many small cubbyholes, all empty. The left wall is " +
            "covered with an enormous map, labelled \"Kalamontee Kompleks\", showing two installations connected by " +
            "a long hallway. Near the upper part of this map is a red arrow saying \"Yuu ar heer.\" The right wall " +
            "is covered with a similar map, labelled \"Lawanda Kompleks\", showing two installations, one apparently " +
            "buried deep underground. ";
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["examine", "look in"], ["cubbyholes", "holes", "cubbies", "cubby"]))
            return new PositiveInteractionResult(
                "The cubbyholes look like the kind that are used to hold maps or blueprints. They are all empty now. ");

        return base.RespondToSimpleInteraction(action, context, client);
    }
}