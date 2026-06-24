using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;
using ZorkOne.Command;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Location;

public class CanyonView : LocationWithNoStartingItems
{
    public override string Name => "Canyon View";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are at the top of the Great Canyon on its west wall. From here there is a marvelous " +
               "view of the canyon and parts of the Frigid River upstream. Across the canyon, the walls of the " +
               "White Cliffs join the mighty ramparts of the Flathead Mountains to the east. Following the Canyon " +
               "upstream to the north, Aragain Falls may be seen, complete with rainbow. The mighty Frigid River " +
               "flows out from a great dark cavern. To the west and south can be seen an immense forest, stretching for " +
               "miles around. A path leads northwest. It is possible to climb down into the canyon from here.";
    }

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["canyon", "great canyon"],
            "The Great Canyon falls away below, its far wall rising into the White Cliffs and the Flathead Mountains beyond. ",
            "You can't take the canyon. "),
        new(["river", "frigid river"],
            "The mighty Frigid River winds upstream, flowing out of a great dark cavern. ",
            "You can't take the river. ")
    ];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.NW, new MovementParameters { Location = GetLocation<ClearingBehindHouse>() }
            },
            {
                Direction.Down, new MovementParameters { Location = GetLocation<RockyLedge>() }
            },
            {
                Direction.E, new MovementParameters { Location = GetLocation<RockyLedge>() }
            },
            {
                Direction.W, new MovementParameters { Location = GetLocation<ForestThree>() }
            }
        };
    }

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(
        string? input,
        IContext context,
        IGenerationClient client
        )
    {
        var command = input?.ToLowerInvariant().Trim();

        if (command == "climb down")
        {
            context.CurrentLocation = Repository.GetLocation<RockyLedge>();
            var message = Repository.GetLocation<RockyLedge>().GetDescription(context);
            return new PositiveInteractionResult(message);
        }

        // Leaping from the canyon's edge is fatal in the original (CANYON-VIEW-F).
        // Note: descending safely is "climb down" / "down", handled separately above and via the Map.
        if (command is not null && Verbs.JumpVerbs.Contains(command))
        {
            var death = "Nice view, lousy place to jump.\n";
            return new DeathProcessor().Process(death, context);
        }

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }
}