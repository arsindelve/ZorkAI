using GameEngine.Location;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

internal class ConferenceRoom : LocationBase
{
    public override string Name => "Conference Room";

    private ConferenceRoomDoor Door => Repository.GetItem<ConferenceRoomDoor>();

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<BoothOne>() },
            {
                Direction.S, new MovementParameters
                {
                    CanGo = _ => Door.IsOpen,
                    Location = GetLocation<RecArea>(),
                    CustomFailureMessage = "The door is closed. "
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a fairly large room, almost filled by a round conference table. To the south is a door which " +
            $"is {(Door.IsOpen ? "open" : "closed")}. To the north is a small room about the size of a phone booth. ";
    }

    public override void Init()
    {
        StartWithItem<ConferenceRoomDoor>();
    }
}