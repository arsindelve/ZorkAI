namespace ZorkOne.Location;

public class Dam : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.SW, new MovementParameters { Location = GetLocation<Chasm>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<ReservoirSouth>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<DeepCanyon>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<DamLobby>() } }
        };

    protected override string ContextBasedDescription =>
        "You are standing on the top of the Flood Control Dam #3, which was quite a tourist attraction in times far distant. " +
        "There are paths to the north, south, and west, and a scramble down.\n\nThe sluice gates on the dam are " +
        "closed. Behind the dam, there can be seen a wide reservoir. Water is pouring over the top of the now abandoned dam. ";

    public override string Name => "Dam";
}