using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;
using ZorkOne.Command;

namespace ZorkOne.Location;

public class Reservoir : DarkLocation, ITurnBasedActor
{
    public override string Name => "Reservoir";

    // "lake" lives only on the central lake bed, not the two shores (Reservoir North/South). The bed
    // forks to both shores, so a shared "lake" alias produced a "which shore?" prompt (issue #268
    // review). From a shore, this central room is the sole "lake" neighbour, so it resolves cleanly.
    public override string[] NounsForMatching => ["lake"];

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        var south = GetLocation<ReservoirSouth>();

        if (south is { IsFilling: false, IsFull: false })
            return Task.FromResult(string.Empty);

        if (south.IsFull)
        {
            context.RemoveActor(this);
            return Task.FromResult(new DeathProcessor().Process(
                "You are lifted up by the rising river! You try to swim, but the currents are too strong. " +
                "You come closer, closer to the awesome structure of Flood Control Dam #3. The dam beckons to you. " +
                "The roar of the water nearly deafens you, but you remain conscious as you tumble over the dam toward " +
                "your certain doom among the rocks at its base.",
                context).InteractionMessage);
        }

        return
            Task.FromResult("You notice that the water level here is rising rapidly. The currents are also becoming " +
                            "stronger. Staying here seems quite perilous! ");
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, new MovementParameters { Location = GetLocation<ReservoirSouth>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<ReservoirNorth>() } },
            // ZIL RESERVOIR: UP and WEST both lead up the stream into the optional Stream branch.
            { Direction.Up, new MovementParameters { Location = GetLocation<InStream>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<InStream>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are on what used to be a large lake, but which is now a large mud pile. There are \"shores\" to the north and south.";
    }

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["mud", "mud pile"],
            "What was once a lake is now a vast expanse of drying mud. ",
            "You could scoop up a handful, but to what end? ")
    ];

    public override void Init()
    {
        StartWithItem<TrunkOfJewels>();
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        context.RegisterActor(this);
        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }
}