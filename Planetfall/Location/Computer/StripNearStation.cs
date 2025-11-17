using GameEngine.Location;

namespace Planetfall.Location.Computer;

internal class StripNearStation : LocationWithNoStartingItems
{
    public override string Name => "Strip Near Station";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, new MovementParameters{ CanGo = _ => false, CustomFailureMessage = "The plunge would probably be fatal." }  },
            { Direction.N, Go<MiddleOfStrip>() }
        };
    }
    
    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(4);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are standing on a silicon filament, which looks to you like a wide metal highway. South of here, the filament makes a right angle and heads straight down, like a " +
            "cliff overlooking a black void. The filament can be followed north, however. Station 384 lies westward. ";
    }
}