using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.CoalMineLocation;

namespace ZorkOne.Location;

internal class DraftyRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.E, new MovementParameters { Location = GetLocation<TimberRoom>(), WeightLimit = 2 }
        },
        {
            Direction.S, new MovementParameters { Location = GetLocation<MachineRoom>() }
        }
    };

    protected override string ContextBasedDescription =>
        "This is a small drafty room in which is the bottom of a long shaft. To the south is a passageway and to " +
        "the east a very narrow passage. In the shaft can be seen a heavy iron chain.  \n"
        + (GetItem<Basket>().Items.Any() ? GetItem<Basket>().ItemListDescription("basket") : "");

    public override string Name => "Drafty Room";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
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

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override void Init()
    {
        StartWithItem<Chain>(this);
    }
}