using ZorkOne.Command;

namespace ZorkOne.Location;

public class Reservoir : DarkLocation, ITurnBasedActor
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<ReservoirSouth>() } }
        };

    protected override string ContextBasedDescription =>
        "You are on what used to be a large lake, but which is now a large mud pile. There are \"shores\" to the north and south.";

    public override string Name => "Reservoir";

    public string Act(IContext context)
    {
        var south = GetLocation<ReservoirSouth>();

        if (south is { IsFilling: false, IsFull: false })
            return string.Empty;

        if (south.IsFull)
        {
            context.RemoveActor(this);
            return new DeathProcessor().Process(
                "You are lifted up by the rising river! You try to swim, but the currents are too strong. " +
                "You come closer, closer to the awesome structure of Flood Control Dam #3. The dam beckons to you. " +
                "The roar of the water nearly deafens you, but you remain conscious as you tumble over the dam toward " +
                "your certain doom among the rocks at its base.",
                context).InteractionMessage;
        }

        return
            "You notice that the water level here is rising rapidly. The currents are also becoming " +
            "stronger. Staying here seems quite perilous! ";
    }

    public override void Init()
    {
    }

    public override string AfterEnterLocation(IContext context)
    {
        context.RegisterActor(this);
        return base.AfterEnterLocation(context);
    }

    public override void OnLeaveLocation(IContext context)
    {
        context.RemoveActor(this);
        base.OnLeaveLocation(context);
    }
}