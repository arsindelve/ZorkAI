using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class LadderBottom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.Up, new MovementParameters { Location = GetLocation<LadderTop>() }
            },
            {
                Direction.S, new MovementParameters { Location = GetLocation<DeadEnd>() }
            },
            {
                Direction.W, new MovementParameters { Location = GetLocation<TimberRoom>() }
            }
        };

    protected override string ContextBasedDescription =>
        "This is a rather wide room. On one side is the bottom of a narrow wooden ladder. To the west and the south are passages leaving the room. ";

    public override string Name => "Ladder Bottom";

    public override void Init()
    {
        StartWithItem<WoodenLadder>(this);
    }
}