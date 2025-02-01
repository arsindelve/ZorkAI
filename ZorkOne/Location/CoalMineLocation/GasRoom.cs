using System.Text;
using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;
using ZorkOne.Command;

namespace ZorkOne.Location.CoalMineLocation;

internal class GasRoom : DarkLocation, ITurnBasedActor, IThiefMayVisit
{
    public override string Name => "Gas Room";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (CarryingFlame(context))
            return Task.FromResult(YouBlewUp(context));

        return Task.FromResult(string.Empty);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.Up, new MovementParameters { Location = GetLocation<SmellyRoom>() }
            },
            {
                Direction.E, new MovementParameters { Location = GetLocation<CoalMineOne>() }
            }
        };
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        context.RegisterActor(this);
        return base.BeforeEnterLocation(context, previousLocation);
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }

    private string YouBlewUp(IContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine(
            "Oh dear. It appears that the smell coming from this room was coal gas. I would have thought twice about carrying flaming objects in here.");
        sb.AppendLine();
        sb.AppendLine("    ** BOOOOOOOOOOOM **");
        return new DeathProcessor().Process(sb.ToString(), context).InteractionMessage;
    }

    private bool CarryingFlame(IContext context)
    {
        // The torch is always lit, so we can cheat and not check if it's lit. But it took more
        // time to write this comment than it would have taken to check if the torch is lit
        // so good call. 
        if (context.HasItem<Torch>())
            return true;

        if (context.HasItem<Candles>() && Repository.GetItem<Candles>().IsOn)
            return true;

        if (context.HasItem<Matchbook>() && Repository.GetItem<Matchbook>().IsOn)
            return true;

        return false;
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a small room which smells strongly of coal gas. There is a short climb up some stairs " +
               "and a narrow tunnel leading east. ";
    }

    public override void Init()
    {
        StartWithItem<SapphireBracelet>();
    }
}