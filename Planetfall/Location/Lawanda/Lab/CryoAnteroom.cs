using GameEngine.Location;

namespace Planetfall.Location.Lawanda.Lab;

internal class CryoAnteroom : LocationWithNoStartingItems, IFloydDoesNotTalkHere
{
    public override string Name => "Cryo-Anteroom";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>();
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "The elevator closes as you leave it, and you find yourself in a small, chilly room. To the north, through a wide arch, " +
            "is an enormous chamber lined from floor to ceiling with thousands of cryo-units. You can see similar chambers beyond, " +
            "and your mind staggers at the thought of the millions of individuals asleep for countless centuries.\n\n" +
            "In the anteroom where you stand is a solitary cryo-unit, its cover frosted. Next to the cryo-unit is a complicated control panel. ";
    }
}
