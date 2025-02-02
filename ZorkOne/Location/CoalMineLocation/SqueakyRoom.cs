using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

public class SqueakyRoom : DarkLocationWithNoStartingItems, IThiefMayVisit
{
    public override string Name => "Squeaky Room ";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.E, new MovementParameters { Location = GetLocation<MineEntrance>() }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<BatRoom>() }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are in a small room. Strange squeaky sounds may be heard coming from the passage at the north end. You may also escape to the east. ";
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var glow = LocationHelper.CheckSwordGlowingFaintly<Bat, BatRoom>(context);
        return !string.IsNullOrEmpty(glow)
            ? Task.FromResult(glow)
            : base.AfterEnterLocation(context, previousLocation, generationClient);
    }
}