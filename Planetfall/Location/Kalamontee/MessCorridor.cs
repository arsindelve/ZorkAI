using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

internal class MessCorridor : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
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

    protected override string ContextBasedDescription =>
        "This is a wide, east-west hallway with a large portal to the south. ";

    public override string Name => "Mess Corridor";

    public override void Init()
    {
        StartWithItem<MessDoor>();
        StartWithItem<Padlock>();
    }
}