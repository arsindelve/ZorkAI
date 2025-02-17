using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class DraftyRoom : DarkLocation, IThiefMayVisit
{
    public override string Name => "Drafty Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.E, new MovementParameters { Location = GetLocation<TimberRoom>(), WeightLimit = 2 }
            },
            {
                Direction.S, new MovementParameters { Location = GetLocation<MachineRoom>() }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a small drafty room in which is the bottom of a long shaft. To the south is a passageway and to " +
            $"the east a very narrow passage. In the shaft can be seen a heavy iron chain. {(Items.Contains(Repository.GetItem<Basket>()) ? "\nAt the end of the chain is a basket. "
                + (GetItem<Basket>().Items.Any() ? GetItem<Basket>().ItemListDescription("basket", this) : "") : "")} \n";
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        var basket = Repository.GetItem<Basket>();
        if (action.Match(["raise"], basket.NounsForMatching))
        {
            if (Items.Contains(basket))
            {
                Repository.GetLocation<ShaftRoom>().ItemPlacedHere(basket);
                return new PositiveInteractionResult("The basket is raised to the top of the shaft. ");
            }

            return new PositiveInteractionResult("Too late for that. ");
        }

        if (action.Match(["lower"], basket.NounsForMatching))
        {
            if (Items.Contains(basket))
                return new PositiveInteractionResult("Too late for that. ");

            ItemPlacedHere(basket);
            return new PositiveInteractionResult("The basket is lowered to the bottom of the shaft. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override void Init()
    {
        StartWithItem<Chain>();
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(13);
        base.OnFirstTimeEnterLocation(context);
    }
}