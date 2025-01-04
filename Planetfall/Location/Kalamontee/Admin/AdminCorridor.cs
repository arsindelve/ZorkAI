using Model.Movement;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee.Admin;

internal class AdminCorridor : RiftLocationBase
{
    public override string Name => "Admin Corridor";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<AdminCorridorSouth>() },
            { Direction.W, Go<SystemsMonitors>() },
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => GetItem<Ladder>().IsAcrossRift,
                    CustomFailureMessage = "The rift is too wide to jump across. ",
                    Location = Repository.GetLocation<AdminCorridorNorth>()
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "The hallway, in fact the entire building, has been rent apart here, presumably by seismic upheaval. " +
               "You can see the sky through the severed roof above, and the ground is thick with rubble. To the north " +
               $"is a gaping rift, at least eight meters across and thirty meters deep. {(GetItem<Ladder>().IsAcrossRift ? "A metal ladder spans the rift." : "")} " +
               "A wide doorway, labelled \"Sistumz Moniturz,\" leads west. ";
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        if (previousLocation is AdminCorridorNorth)
            return
                "You slowly make your way across the swaying ladder. You can see sharp, pointy rocks at the bottom of the rift, far below...\n\n";

        return base.BeforeEnterLocation(context, previousLocation);
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

            ladder.IsAcrossRift = true;
            // Like the Zork kitchen window, the ladder exists in both places now. 
            GetLocation<AdminCorridorNorth>().Items.Add(ladder);
            return new PositiveInteractionResult(
                "The ladder swings out across the rift and comes to rest on the far edge, spanning the precipice. ");
        }

        return base.RespondToMultiNounInteraction(action, context);
    }
}