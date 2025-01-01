using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

public class MineEntrance : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.S, new MovementParameters { Location = GetLocation<SlideRoom>() }
            },
            {
                Direction.W, new MovementParameters { Location = GetLocation<SqueakyRoom>() }
            }
        };

    protected override string GetContextBasedDescription() =>
        "You are standing at the entrance of what might have been a coal mine. The shaft enters the west wall, " +
        "and there is another exit on the south end of the room.";

    public override string Name => "Mine Entrance";

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        return LocationHelper.CheckSwordNoLongerGlowing<Bat, BatRoom, SqueakyRoom>(previousLocation, context,
            base.AfterEnterLocation(context, previousLocation, generationClient));
    }
}