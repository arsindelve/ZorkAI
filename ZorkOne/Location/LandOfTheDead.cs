using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

internal class LandOfTheDead : DarkLocation
{
    public override string Name => "Land of the Dead";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = GetLocation<EntranceToHades>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You have entered the Land of the Living Dead. Thousands of lost souls can be heard weeping and " +
               "moaning. In the corner are stacked the remains of dozens of previous adventurers less fortunate than " +
               "yourself. A passage exits to the north.";
    }

    public override void Init()
    {
        StartWithItem<CrystalSkull>();
    }
}