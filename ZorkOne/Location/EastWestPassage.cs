namespace ZorkOne.Location;

public class EastWestPassage : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<TrollRoom>() } }
            
        };

    protected override string Name => "East-West Passage";

    protected override string ContextBasedDescription =>
        "This is a narrow east-west passageway. There is a narrow stairway leading down at the north end of the room.";
}