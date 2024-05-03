namespace ZorkOne.Location;

internal class EastWestPassage : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<TrollRoom>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<RoundRoom>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<Chasm>() } },
            { Direction.Down, new MovementParameters { Location = GetLocation<Chasm>() } }
        };

    public override string Name => "East-West Passage";

    protected override string ContextBasedDescription =>
        "This is a narrow east-west passageway. There is a narrow stairway leading down at the north end of the room. ";
    
    public override string AfterEnterLocation(IContext context, ILocation previousLocation)
    {
        var swordInPossession = context.HasItem<Sword>();
        var trollIsAlive = Repository.GetItem<Troll>().CurrentLocation == Repository.GetLocation<TrollRoom>();

        if (trollIsAlive && swordInPossession && previousLocation is Cellar)
            return "\nYour sword is glowing with a faint blue glow.";

        return base.AfterEnterLocation(context, previousLocation);
    }
    
    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(5);
        base.OnFirstTimeEnterLocation(context);
    }
}