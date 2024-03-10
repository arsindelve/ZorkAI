using Game;
using Game.Location;
using Model;
using ZorkOne.Item;

namespace ZorkOne.Location;

public class WestOfHouse : BaseLocation
{
    protected override string ContextBasedDescription =>
        "You are standing in an open field west of a white house, with a boarded front door. ";

    protected override string Name => "West Of House";

    public WestOfHouse()
    {
        StartWithItem(Repository.GetItem<Mailbox>(), this);
    }

    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.S, new MovementParameters { Location = GetLocation<SouthOfHouse>() }
        },
        {
            Direction.N, new MovementParameters { Location = GetLocation<NorthOfHouse>() }
        },
        {
            Direction.E,
            new MovementParameters
            {
                CanGo = _ => false,
                CustomFailureMessage = "The door is boarded and you can't remove the boards."
            }
        }
    };
}