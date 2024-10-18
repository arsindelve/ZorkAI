using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

public class SqueakyRoom : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.E, new MovementParameters { Location = GetLocation<MineEntrance>() }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<BatRoom>() }
            }
        };

    protected override string ContextBasedDescription =>
        "You are in a small room. Strange squeaky sounds may be heard coming from the passage at the north end. You may also escape to the east. ";

    public override string Name => "Squeaky Room ";
    
    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient? generationClient)
    {
        var swordInPossession = context.HasItem<Sword>();

        if (swordInPossession)
            return Task.FromResult("\nYour sword is glowing with a faint blue glow.");

        return Task.FromResult(string.Empty);
    }
}