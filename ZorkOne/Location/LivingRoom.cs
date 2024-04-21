namespace ZorkOne.Location;

public class LivingRoom : BaseLocation
{
    protected override string ContextBasedDescription =>
        $"You are in the living room. There is a doorway to the east, a wooden door with strange gothic lettering to the west, " +
        $"which appears to be nailed shut, a trophy case, " +
        $"{(Repository.GetItem<Rug>().HasBeenMovedAside
            ? $"and a rug lying beside {(Repository.GetItem<TrapDoor>().IsOpen ? "an open" : "a closed")} trap door. "
            : "and a large oriental rug in the center of the room. ")}{GetItem<TrophyCase>().ItemListDescription("sack")}";

    public override string Name => "Living Room";

    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.E, new MovementParameters { Location = GetLocation<Kitchen>() }
        },
        {
            Direction.W,
            new MovementParameters
            {
                CanGo = _ => false,
                CustomFailureMessage = "The door is nailed shut."
            }
        },
        {
            Direction.Down,
            new MovementParameters
            {
                Location = GetLocation<Cellar>(),
                CanGo = _ =>
                    Repository.GetLocation<LivingRoom>().HasItem<TrapDoor>() && Repository.GetItem<TrapDoor>().IsOpen,
                CustomFailureMessage = Repository.GetLocation<LivingRoom>().HasItem<TrapDoor>()
                    ? "The trap door is closed."
                    : "You can't go that way."
            }
        }
    };

    public override void Init()
    {
        StartWithItem<Sword>(this);
        StartWithItem<Lantern>(this);
        StartWithItem<Rug>(this);
        StartWithItem<TrophyCase>(this);
    }
}