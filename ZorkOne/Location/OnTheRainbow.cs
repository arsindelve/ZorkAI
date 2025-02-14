using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using ZorkOne.Command;

namespace ZorkOne.Location;

public class OnTheRainbow : LocationWithNoStartingItems
{
    public override string Name => "On The Rainbow";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.W, Go<EndOfRainbow>()
            },
            {
                Direction.E, Go<AragainFalls>()
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are on top of a rainbow (I bet you never thought you would walk on a rainbow), " +
               "with a magnificent view of the Falls. The rainbow travels east-west here.";
    }
    
    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["wave", "swing", "twirl"], GetItem<Sceptre>().NounsForMatching))
        {
            if (!context.HasItem<Sceptre>() && GetItem<Sceptre>().CurrentLocation == GetLocation<EndOfRainbow>())
                return new PositiveInteractionResult("You don't have the sceptre. ");

            if (!context.HasItem<Sceptre>())
                return new NoNounMatchInteractionResult();

            string deathString =
                "The structural integrity of the rainbow is severely compromised, leaving you hanging in mid-air, " +
                "supported only by water vapor. Bye. ";
            
            return new DeathProcessor().Process(deathString, context);
        }
        
        return base.RespondToSimpleInteraction(action, context, client);
    }
}