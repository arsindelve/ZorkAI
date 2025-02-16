using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class EndOfRainbow : LocationWithNoStartingItems
{
    public bool RainbowIsSolid { get; set; }

    public override string Name => "End of Rainbow";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.SW, new MovementParameters { Location = GetLocation<CanyonBottom>() }
            },
            {
                Direction.E,
                new MovementParameters { Location = GetLocation<OnTheRainbow>(), CanGo = _ => RainbowIsSolid }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are on a small, rocky beach on the continuation of the Frigid River past the Falls. The beach is narrow " +
            "due to the presence of the White Cliffs. The river canyon opens here and sunlight shines in from above. " +
            "A rainbow crosses over the falls to the east and a narrow path continues to the southwest. ";
    }


    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["cross", "traverse"], ["rainbow"]))
        {
            if (!GetLocation<EndOfRainbow>().RainbowIsSolid)
                return new PositiveInteractionResult("Can you walk on water vapor? ");

            context.CurrentLocation = GetLocation<AragainFalls>();
            return new PositiveInteractionResult(context.CurrentLocation.GetDescription(context));
        }

        if (action.Match(["wave", "swing", "twirl"], GetItem<Sceptre>().NounsForMatching))
        {
            if (!context.HasItem<Sceptre>() && GetItem<Sceptre>().CurrentLocation == GetLocation<EndOfRainbow>())
                return new PositiveInteractionResult("You don't have the sceptre. ");

            if (!context.HasItem<Sceptre>())
                return new NoNounMatchInteractionResult();

            return WaveTheSceptre();
        }
        
        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    private InteractionResult WaveTheSceptre()
    {
        if (RainbowIsSolid)
        {
            RainbowIsSolid = false;
            return new PositiveInteractionResult("The rainbow seems to have become somewhat run-of-the-mill.");
        }

        RainbowIsSolid = true;

        var oldLocation = Repository.GetItem<PotOfGold>().CurrentLocation;

        if (oldLocation == null)
            ItemPlacedHere(GetItem<PotOfGold>());

        return new PositiveInteractionResult(
            "Suddenly, the rainbow appears to become solid and, I venture, walkable (I think " +
            "the giveaway was the stairs and bannister). " + (oldLocation != null
                ? ""
                : " A shimmering pot of gold appears at the end of the rainbow. \n"));
    }
}