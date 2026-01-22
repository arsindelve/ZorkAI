using Model.AIGeneration;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Location.Lawanda;

internal class ProjConOffice : FloydSpecialInteractionLocation
{
    public override string Name => "ProjCon Office";

    public override string FloydPrompt => FloydPrompts.ProjConOfficeMural;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var relay = Repository.GetItem<Relay>();
        var door = Repository.GetItem<CryoElevatorDoor>();

        var map = new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<ProjectCorridor>() },
            { Direction.E, Go<ComputerRoom>() }
        };

        if (relay.SpeckDestroyed)
        {
            // Make door visible and open when computer is fixed
            door.IsVisible = true;
            door.IsOpen = true;

            map.Add(Direction.S, new MovementParameters
            {
                CanGo = _ => door.IsOpen,
                CustomFailureMessage = "The elevator door is closed. ",
                Location = GetLocation<CryoElevatorLocation>()
            });
        }

        return map;
    }

    public override void Init()
    {
        StartWithItem<Mural>();
        StartWithItem<CryoElevatorDoor>();
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

        if (relay.SpeckDestroyed)
        {
            return
                "This office looks like a headquarters of some kind. Exits lead north and east. The west wall displays a logo. " +
                "The mural that previously adorned the south wall has slid away, revealing an open doorway to a large elevator! ";
        }

        return
            "This office looks like a headquarters of some kind. Exits lead north and east. The west wall displays a " +
            "logo. The south wall is completely covered by a garish mural which clashes with the other decor of the room. ";
    }
}