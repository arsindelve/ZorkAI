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
        var relay = Repository.GetItem<Relay>();
        var eastMessage = relay.RelayDestroyed
            ? "You would slice yourself to ribbons on the shattered relay. "
            : "The relay is sealed. Although you cannot enter it, you could look into it. ";

        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, new MovementParameters{ CanGo = _ => false, CustomFailureMessage = "Do you have a penchant for bottomless voids? " }  },
            { Direction.E, new MovementParameters{ CanGo = _ => false, CustomFailureMessage = eastMessage }  },
            { Direction.S, Go<MiddleOfStrip>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        var relay = Repository.GetItem<Relay>();

        if (relay.RelayDestroyed)
            return
                "North of here, the filament ends at a huge featureless wall, presumably the side of some microcomponent. " +
                "To the east are the shattered remains of the microrelay. ";

        return
            "North of here, the filament ends at a huge featureless wall, presumably the side of some microcomponent. " +
            "To the east is a vacuu-sealed microrelay, sealed in transparent red plastic. You could probably see into the microrelay. ";
    }
}