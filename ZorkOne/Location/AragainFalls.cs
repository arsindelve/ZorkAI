using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class AragainFalls : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        { Direction.N, Go<Shore>() },
        {
            Direction.W,
            new MovementParameters
                { CanGo = _ => GetLocation<EndOfRainbow>().RainbowIsSolid, Location = GetLocation<OnTheRainbow>() }
        }
    };

    protected override string GetContextBasedDescription() =>
        "You are at the top of Aragain Falls, an enormous waterfall with a drop of about 450 feet. The only path " +
        "here is on the north end. \n" + (GetLocation<EndOfRainbow>().RainbowIsSolid
            ? "A solid rainbow spans the falls. "
            : "A beautiful rainbow can be seen over the falls and to the west. ");

    public override string Name => "Aragain Falls";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["cross"], ["rainbow"]))
        {
            if (!GetLocation<EndOfRainbow>().RainbowIsSolid)
                return new PositiveInteractionResult("Can you walk on water vapor? ");

            context.CurrentLocation = GetLocation<EndOfRainbow>();
            return new PositiveInteractionResult(context.CurrentLocation.Description);
        }

        return base.RespondToSimpleInteraction(action, context, client);
    }
}