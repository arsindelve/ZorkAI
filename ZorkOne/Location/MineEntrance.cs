namespace ZorkOne.Location;

public class MineEntrance : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.S, new MovementParameters { Location = GetLocation<SlideRoom>() }
            },
            {
                Direction.W, new MovementParameters { Location = GetLocation<SqueakyRoom>() }
            }
        };

    protected override string ContextBasedDescription =>
        "You are standing at the entrance of what might have been a coal mine. The shaft enters the west wall, " +
        "and there is another exit on the south end of the room.";

    public override string Name => "Mine Entrance";
    
    public override string AfterEnterLocation(IContext context, ILocation previousLocation)
    {
        var swordInPossession = context.HasItem<Sword>();

        if (swordInPossession && previousLocation is SqueakyRoom)
            return "\n\nYour sword is no longer glowing. ";

        return base.AfterEnterLocation(context, previousLocation);
    }
}