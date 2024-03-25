namespace ZorkOne.Location;

public class Chasm : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.SW, new MovementParameters { Location = GetLocation<EastWestPassage>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<NorthSouthPassage>() } },
            { Direction.NE, new MovementParameters { Location = GetLocation<ReservoirSouth>() } }
        };

    protected override string ContextBasedDescription =>
        "A chasm runs southwest to northeast and the path follows it. You are on the south side of the " +
        "chasm, where a crack opens into a passage. ";

    public override string Name => "Chasm";

    public override void Init()
    {
    }
}