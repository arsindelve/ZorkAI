using GameEngine;
using GameEngine.Location;
using Model.Interface;
using Model.Movement;
using ZorkOne.Item;

namespace ZorkOne.Location;

public class WhiteCliffsBeachNorth : LocationWithNoStartingItems
{
    public override string Name => "White Cliffs Beach";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.W, new MovementParameters
                {
                    Location = GetLocation<DampCave>(), CanGo = ctx => !InflatedBoatIsWithPlayer(ctx),
                    CustomFailureMessage = "The path is too narrow. "
                }
            },
            {
                Direction.S, new MovementParameters
                {
                    Location = GetLocation<WhiteCliffsBeachSouth>(), CanGo = ctx => !InflatedBoatIsWithPlayer(ctx),
                    CustomFailureMessage = "The path is too narrow. "
                }
            }
        };
    }

    // Mirrors the ZIL DEFLATE flag (zork1/1actions.zil:2585-2590): the cliff paths are
    // passable only while the inflated boat isn't with the player, whether seated in it
    // or simply carrying it.
    private bool InflatedBoatIsWithPlayer(IContext context)
    {
        var boat = Repository.GetItem<PileOfPlastic>();
        return boat.IsInflated && (SubLocation == boat || context.HasItem<PileOfPlastic>());
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are on a narrow strip of beach which runs along the base of the White Cliffs. There is a narrow " +
               "path heading south along the Cliffs and a tight passage leading west into the cliffs themselves. ";
    }

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["cliffs", "white cliffs", "cliff"],
            "The White Cliffs rise sheer and pale above the narrow strip of beach. ",
            "You can't take the cliffs. ")
    ];
}