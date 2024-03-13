namespace ZorkOne.Location;

public class Cellar : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.S, new MovementParameters { Location = GetLocation<EastOfChasm>() }
        },
        {
            Direction.N, new MovementParameters { Location = GetLocation<TrollRoom>() }
        },
        {
            Direction.W,
            new MovementParameters
            {
                CanGo = _ => false,
                CustomFailureMessage = "You try to ascend the ramp, but it is impossible, and you slide back down."
            }
        },
        {
            Direction.Up,
            new MovementParameters
            {
                CanGo = _ => false,
                CustomFailureMessage = "The trap door is closed. "
            }
        }
    };

    protected override string Name => "Cellar";

    protected override string ContextBasedDescription => "You are in a dark and damp cellar with a narrow passageway " +
                                                         "leading north, and a crawlway to the south. On the west is the " +
                                                         "bottom of a steep metal ramp which is unclimbable. ";

    public override string BeforeEnterLocation(IContext context)
    {
        var result = base.BeforeEnterLocation(context);

        if (Repository.GetItem<TrapDoor>().IsOpen)
        {
            Repository.GetItem<TrapDoor>().IsOpen = false;
            result += "The trap door crashes shut, and you hear someone barring it." + Environment.NewLine +
                      Environment.NewLine;
        }

        return result;
    }

    public override string AfterEnterLocation(IContext context)
    {
        var swordInPossession = context.HasItem<Sword>();
        var trollIsAlive = Repository.GetItem<Troll>().CurrentLocation == Repository.GetLocation<TrollRoom>();

        if (trollIsAlive && swordInPossession)
            return "\nYour sword is glowing with a faint blue glow.";

        return string.Empty;
    }
}