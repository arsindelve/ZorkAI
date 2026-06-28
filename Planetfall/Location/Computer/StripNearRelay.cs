using GameEngine.Location;
using Model.AIGeneration;
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
            {
                Direction.S,
                Repository.GetItem<Microbe>().IsActive
                    ? new MovementParameters { CanGo = _ => false, CustomFailureMessage = Microbe.BlocksExitMessage }
                    : Go<MiddleOfStrip>()
            }
        };
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        // The microbe, writhing angrily, follows you northward (STRIP-NEAR-RELAY-F M-ENTER, comptwo.zil:2524).
        var microbe = Repository.GetItem<Microbe>();
        if (microbe.IsActive && microbe.CurrentLocation != this)
        {
            var followText = microbe.FollowInto(this);
            if (followText is not null)
                return Task.FromResult(followText);
        }

        return base.AfterEnterLocation(context, previousLocation, generationClient);
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