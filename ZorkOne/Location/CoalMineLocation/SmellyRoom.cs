using GameEngine.Location;
using Model.AIGeneration;
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

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        return LocationHelper.CheckSwordNoLongerGlowing<Bat, BatRoom, ShaftRoom>(previousLocation, context,
            base.AfterEnterLocation(context, previousLocation, generationClient));
    }
}