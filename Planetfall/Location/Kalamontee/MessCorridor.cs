using GameEngine.Location;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

internal class MessCorridor : LocationBase
{
    public override string Name => "Mess Corridor";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<DormCorridor>() },
            { Direction.S, Go<MessHall>() },
            { Direction.W, Go<RecCorridor>() },
            {
                Direction.N, new MovementParameters
                {
                    Location = Repository.GetLocation<StorageWest>(),
                    CanGo = _ => Repository.GetItem<MessDoor>().IsOpen,
                    CustomFailureMessage = Repository.GetItem<MessDoor>().IsOpen ? "" : "The door is closed. "
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a wide, east-west hallway with a large portal to the south. ";
    }

    public override void Init()
    {
        StartWithItem<MessDoor>();
        StartWithItem<Padlock>();
    }
}