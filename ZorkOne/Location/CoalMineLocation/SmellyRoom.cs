using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class SmellyRoom : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.S, new MovementParameters { Location = GetLocation<ShaftRoom>() }
            },
            {
                Direction.Down, new MovementParameters { Location = GetLocation<GasRoom>() }
            }
        };

    protected override string ContextBasedDescription =>
        "This is a small nondescript room. However, from the direction of a small " +
        "descending staircase a foul odor can be detected. To the south is a narrow tunnel.";

    public override string Name => "Smelly Room";

    public override string AfterEnterLocation(IContext context, ILocation previousLocation)
    {
        var swordInPossession = context.HasItem<Sword>();

        if (swordInPossession && previousLocation is ShaftRoom)
            return "\n\nYour sword is no longer glowing. ";

        return base.AfterEnterLocation(context, previousLocation);
    }
}