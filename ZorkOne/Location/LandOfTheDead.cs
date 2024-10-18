using GameEngine.Location;
using Model.Movement;

namespace ZorkOne.Location;

internal class LandOfTheDead : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<EntranceToHades>() } }
        };
    
    protected override string ContextBasedDescription =>
        "You have entered the Land of the Living Dead. Thousands of lost souls can be heard weeping and " +
        "moaning. In the corner are stacked the remains of dozens of previous adventurers less fortunate than " +
        "yourself. A passage exits to the north.";

    public override string Name => "Land of the Dead";

    public override void Init()
    {
        StartWithItem<CrystalSkull>();
    }
}