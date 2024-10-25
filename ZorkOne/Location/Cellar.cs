using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

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
                CanGo = _ => GetItem<TrapDoor>().IsOpen,
                CustomFailureMessage = "The trap door is closed. ",
                Location = GetLocation<LivingRoom>()
            }
        }
    };

    public override string Name => "Cellar";

    protected override string ContextBasedDescription => "You are in a dark and damp cellar with a narrow passageway " +
                                                         "leading north, and a crawlway to the south. On the west is the " +
                                                         "bottom of a steep metal ramp which is unclimbable. ";

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        var result = base.BeforeEnterLocation(context, previousLocation);

        if (Repository.GetItem<TrapDoor>().IsOpen && VisitCount == 1)
        {
            Repository.GetItem<TrapDoor>().IsOpen = false;
            result += "The trap door crashes shut, and you hear someone barring it." + Environment.NewLine +
                      Environment.NewLine;
        }

        return result;
    }

    public override void Init()
    {
        StartWithItem<TrapDoor>();
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var glow = LocationHelper.CheckSwordGlowingFaintly<Troll, TrollRoom>(context);
        return !string.IsNullOrEmpty(glow)
            ? Task.FromResult(glow)
            : base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(25);
        base.OnFirstTimeEnterLocation(context);
    }
}