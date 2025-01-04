using GameEngine;
using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class Kitchen : LocationBase
{
    public override string Name => "Kitchen";

    protected override string GetContextBasedDescription(IContext context)
    {
        return $"You are in the kitchen of the white house. A table seems to have been " +
               $"used recently for the preparation of food. A passage leads to the west " +
               $"and a dark staircase can be seen leading upward. A dark chimney leads down " +
               $"and to the east is a small window which is {(GetItem<KitchenWindow>().IsOpen ? "open" : "closed")}. ";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var exit = new MovementParameters
        {
            CanGo = _ => GetItem<KitchenWindow>().IsOpen,
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

    public override void Init()
    {
        StartWithItem<KitchenWindow>();
        StartWithItem<BrownSack>();
        StartWithItem<Bottle>();
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(10);
        base.OnFirstTimeEnterLocation(context);
    }
}