using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Command;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee.Admin;

public class AdminCorridor : LocationWithNoStartingItems
{
    internal static string[] RiftNouns = ["rift", "gap", "split", "crack", "fissure", "break", "chasm"];
    public bool LadderAcrossRift { get; set; }

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, Go<AdminCorridorSouth>() },
            { Direction.W, Go<SystemsMonitors>() },
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => LadderAcrossRift,
                    CustomFailureMessage = "The rift is too wide to jump across. ",
                    Location = Repository.GetLocation<AdminCorridorNorth>()
                }
            }
        };

    protected override string ContextBasedDescription =>
        "The hallway, in fact the entire building, has been rent apart here, presumably by seismic upheaval. " +
        "You can see the sky through the severed roof above, and the ground is thick with rubble. To the north " +
        "is a gaping rift, at least eight meters across and thirty meters deep. A wide doorway, " +
        "labelled \"Sistumz Moniturz,\" leads west. ";

    public override string Name => "Admin Corridor South";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["jump", "leap"], RiftNouns))
            return new DeathProcessor().Process(
                "You get a brief (but much closer) view of the sharp and nasty rocks at the bottom of the rift. ",
                context);

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        var ladder = GetItem<Ladder>();
        string[] verbs = ["move", "place", "put"];
        var nounOne = ladder.NounsForMatching;
        string[] prepositions = ["across", "over"];

        if (action.Match(verbs, nounOne, RiftNouns, prepositions))
        {
            if (!ladder.IsExtended)
            {
                ladder.CurrentLocation?.Items.Remove(ladder);
                ladder.CurrentLocation = null;
                return new PositiveInteractionResult(
                    "The ladder, far too short to reach the other edge of the rift, plunges into the rift and is lost forever. ");
            }

            LadderAcrossRift = true;
            return new PositiveInteractionResult(
                "The ladder swings out across the rift and comes to rest on the far edge, spanning the precipice. ");
        }

        return base.RespondToMultiNounInteraction(action, context);
    }
}