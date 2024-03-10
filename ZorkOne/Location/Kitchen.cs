using Game;
using Game.Location;
using Model;
using ZorkOne.Item;

namespace ZorkOne.Location;

public class Kitchen : BaseLocation
{
    private static KitchenWindow? _window;

    public Kitchen()
    {
        _window = Repository.GetItem<KitchenWindow>();
        StartWithItem(_window, this);
        StartWithItem(Repository.GetItem<BrownSack>(), this);
        StartWithItem(Repository.GetItem<Bottle>(), this);
    }

    protected override string ContextBasedDescription =>
        $"You are in the kitchen of the white house. A table seems to have been" +
        $" used recently for the preparation of food. A passage leads to the west " +
        $"and a dark staircase can be seen leading upward. A dark chimney leads down " +
        $"and to the east is a small window which is {(Repository.GetItem<KitchenWindow>().IsOpen ? "open" : "closed")}.";

    protected override string Name => "Kitchen";

    protected override Dictionary<Direction, MovementParameters> Map
    {
        get
        {
            var exit = new MovementParameters
            {
                CanGo = _ => _window?.IsOpen ?? false,
                CustomFailureMessage = "The kitchen window is closed.",
                Location = Repository.GetLocation<BehindHouse>()
            };

            return new Dictionary<Direction, MovementParameters>
            {
                {
                    Direction.Up, new MovementParameters { Location = GetLocation<Attic>() }
                },
                {
                    Direction.W, new MovementParameters { Location = GetLocation<LivingRoom>() }
                },
                {
                    Direction.E, exit
                },
                {
                    Direction.Out, exit
                },
                {
                    Direction.Down, new MovementParameters
                    {
                        CanGo = _ => false,
                        CustomFailureMessage = "Only Santa Claus climbs down chimneys."
                    }
                }
            };
        }
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(10);
        base.OnFirstTimeEnterLocation(context);
    }
}