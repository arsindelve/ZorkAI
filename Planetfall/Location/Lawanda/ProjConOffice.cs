using Model.AIGeneration;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Location.Lawanda;

internal class ProjConOffice : FloydSpecialInteractionLocation
{
    public override string Name => "ProjCon Office";
    
    [UsedImplicitly]
    public bool AnnouncmentHasBeenMade { get; set; }

    public override string FloydPrompt => FloydPrompts.ProjConOfficeMural;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var map = new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<ProjectCorridor>() },
            { Direction.E, Go<ComputerRoom>() },
            {
                Direction.S,
                new MovementParameters
                {
                    Location = Repository.GetLocation<CryoElevatorLocation>(),
                    CustomFailureMessage = "You can't go that way.", CanGo = _ => AnnouncmentHasBeenMade
                }
            }
        };

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

        if (action.MatchNoun(["mural", "garish mural"]))
            return new PositiveInteractionResult(
                "It's a gaudy work of orange and purple abstract shapes, reminiscent of the early works of " +
                "Burstini Bonz. It doesn't appear to fit the decor of the room at all. The mural seems to " +
                "ripple now and then, as though a breeze were blowing behind it.");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return AnnouncmentHasBeenMade
            ? "This office looks like a headquarters of some kind. Exits lead north and east. " +
              "The west wall displays a logo. The mural that previously adorned the south " +
              "wall has slid away, revealing an open doorway to a large elevator!"
            : "TThis office looks like a headquarters of some kind. Exits lead north and east. The west wall displays a " +
              "logo. The south wall is completely covered by a garish mural which clashes with the other decor of the room. ";
    }
}