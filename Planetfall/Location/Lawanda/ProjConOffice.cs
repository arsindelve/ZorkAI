using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;

namespace Planetfall.Location.Lawanda;

internal class ProjConOffice : LocationWithNoStartingItems
{
    public override string Name => "ProjCon Office";
    
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<ProjectCorridor>() }
        };
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(["examine", "look at", "look"]))
            return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        if (action.MatchNoun(["logo"]))
            return new PositiveInteractionResult(
                "The logo shows a flame burning over a sleep chamber of some type. Under that is the phrase \"Prajekt Kuntrool.\"");

        if (action.MatchNoun(["mural"]))
            return new PositiveInteractionResult(
                "It's a gaudy work of orange and purple abstract shapes, reminiscent of the early works " +
                "of Burstini Bonz. It doesn't appear to fit the decor of the room at all. The mural seems to ripple " +
                "now and then, as though a breeze were blowing behind it.");

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
    
    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This office looks like a headquarters of some kind. Exits lead north and east. The west wall displays a " +
            "logo. The south wall is completely covered by a garish mural which clashes with the other decor of the room. ";
    }
}