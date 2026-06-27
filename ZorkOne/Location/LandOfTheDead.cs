using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

internal class LandOfTheDead : DarkLocation
{
    public override string Name => "Land of the Dead";

    public override string[] NounsForMatching => ["underworld"];

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

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["souls", "lost souls", "voices", "dead"],
            "The wails of countless lost souls echo from every direction, though none of them can be seen. ",
            "The souls are quite beyond your reach — mercifully. "),
        new(["remains", "adventurers", "bones"],
            "Stacked in the corner are the remains of dozens of adventurers who fared worse than you. ",
            "Best to leave the dead what little dignity they have left. ")
    ];

    public override void Init()
    {
        StartWithItem<CrystalSkull>();
    }
}