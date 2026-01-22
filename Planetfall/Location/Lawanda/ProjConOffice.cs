using Model.AIGeneration;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Location.Lawanda;

internal class ProjConOffice : FloydSpecialInteractionLocation
{
    public override string Name => "ProjCon Office";

    public override string FloydPrompt => FloydPrompts.ProjConOfficeMural;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var relay = Repository.GetItem<Relay>();
        var computerFixed = relay.SpeckDestroyed;

        var map = new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<ProjectCorridor>() }
        };

        // Add east exit to cryo-elevator only when computer is fixed
        if (computerFixed)
        {
            map.Add(Direction.E, Go<CryoElevatorLocation>());
        }

        return map;
    }

    public override void Init()
    {
        StartWithItem<Mural>();
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(["examine", "look at", "look"]))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        if (action.MatchNoun(["logo"]))
            return new PositiveInteractionResult(
                "The logo shows a flame burning over a sleep chamber of some type. Under that is the phrase \"Prajekt Kuntrool.\" ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        var relay = Repository.GetItem<Relay>();
        var computerFixed = relay.SpeckDestroyed;

        var exitDescription = computerFixed
            ? "Exits lead north and east. "
            : "The exit leads north. ";

        return
            $"This office looks like a headquarters of some kind. {exitDescription}The west wall displays a " +
            "logo. The south wall is completely covered by a garish mural which clashes with the other decor of the room. ";
    }
}