using GameEngine.Location;
using Planetfall.Item.Computer;

namespace Planetfall.Location.Computer;

internal class StripNearRelay : LocationBase
{
    public override void Init()
    {
        StartWithItem<Relay>();
    }

    public override string Name => "Strip Near Relay";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, new MovementParameters{ CanGo = _ => false, CustomFailureMessage = "Do you have a penchant for bottomless voids? " }  },
            { Direction.E, new MovementParameters{ CanGo = _ => false, CustomFailureMessage = "The relay is sealed. Although you cannot enter it, you could look into it. " }  },
            { Direction.S, Go<MiddleOfStrip>() }
        };
    }
    
    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(4);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "North of here, the filament ends at a huge featureless wall, presumably the side of some microcomponent. " +
            "To the east is a vacuu-sealed microrelay, sealed in transparent red plastic. You could probably see into the microrelay. ";
    }
}