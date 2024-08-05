using GameEngine;
using GameEngine.Location;
using Model.Movement;

namespace ZorkOne.Location;

public class Gallery : BaseLocation
{
    public override string Name => "Gallery";

    protected override string ContextBasedDescription =>
        "This is an art gallery. Most of the paintings have been stolen by vandals with exceptional taste. " +
        "The vandals left through either the north or west exits. ";

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<EastOfChasm>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<Studio>() } }
        };

    public override void Init()
    {
        StartWithItem<Painting>();
    }
}