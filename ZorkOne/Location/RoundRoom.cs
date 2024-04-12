namespace ZorkOne.Location;

public class RoundRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<EastWestPassage>() } },
            { Direction.SE, new MovementParameters { Location = GetLocation<EngravingsCave>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<NorthSouthPassage>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<LoudRoom>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<NarrowPassage>() } }
        };

    public override string Name => "Round Room";

    protected override string ContextBasedDescription =>
        "This is a circular stone room with passages in all directions. Several of them have unfortunately been blocked by cave-ins.";

    public override void Init()
    {
    }
}