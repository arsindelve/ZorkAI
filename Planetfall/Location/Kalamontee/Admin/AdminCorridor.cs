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

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["roof", "severed roof", "sky"],
            "Through the torn-open roof above, you can see open sky. ",
            "The roof is well out of reach. ")
    ];

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        // Issue #473: prepend the transition text to base (mirroring AdminCorridorNorth) rather than
        // returning it standalone. base.BeforeEnterLocation is the only place VisitCount is
        // incremented (and OnFirstTimeEnterLocation fires); returning early skipped it, so in Brief
        // mode the first-visit room description was silently dropped.
        string prepend = "";
        if (previousLocation is AdminCorridorNorth)
            prepend =
                "You slowly make your way across the swaying ladder. You can see sharp, pointy rocks at the bottom of the rift, far below...\n\n";

        return prepend + base.BeforeEnterLocation(context, previousLocation);
    }


    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        var ladder = GetItem<Ladder>();
        string[] verbs = ["move", "place", "put"];
        var nounOne = ladder.NounsForMatching;
        string[] prepositions = ["across", "over"];

        var match = action.Match(verbs, nounOne, RiftNouns, prepositions);

        // Also support "use ladder on rift"
        match |= action.Match(["use"], nounOne, RiftNouns, ["on"]);

        if (match)
        {
            // action.Match keys off the player's words, not the ladder's presence/state. Guard on
            // the ladder's actual state, mirroring the ZIL LADDER-FCN ordering
            // (planetfall-source/compone.zil:697-712), or we narrate against a ladder that
            // isn't here (issue #297).

            // Already spans the rift — checked first, as the ZIL does (LADDER-FLAG branch). Don't
            // re-narrate success or re-add the instance (Divergence B).
            if (ladder.IsAcrossRift)
                return new PositiveInteractionResult("The ladder already spans the rift. ");

            // The ladder must actually be in scope to bridge the rift: here in the corridor or
            // carried. If it plunged into the rift (Divergence A) or was simply left in another
            // room, the original's parser can't resolve "ladder" to run its action routine at all,
            // so let normal routing answer instead of narrating against a ladder that isn't here.
            if (!ladder.IsHereButNotInInventory(context) && !context.HasItem<Ladder>())
                return await base.RespondToMultiNounInteraction(action, context);

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

        return await base.RespondToMultiNounInteraction(action, context);
    }
}