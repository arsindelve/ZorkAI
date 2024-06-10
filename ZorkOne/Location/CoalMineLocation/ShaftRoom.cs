using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class ShaftRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.W, new MovementParameters { Location = GetLocation<BatRoom>() }
            },
            {
                Direction.Down,
                new MovementParameters
                    { CanGo = _ => false, CustomFailureMessage = "You wouldn't fit and would die if you could. " }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<SmellyRoom>() }
            }
        };

    protected override string ContextBasedDescription =>
        "This is a large room, in the middle of which is a small shaft descending through the floor into darkness " +
        "below. To the west and the north are exits from this room. Constructed over the top of the shaft is a metal " +
        $"framework to which a heavy iron chain is attached. {(Items.Contains(Repository.GetItem<Basket>()) ? "\nAt the end of the chain is a basket. " 
            + (GetItem<Basket>().Items.Any() ? GetItem<Basket>().ItemListDescription("basket") : "") : "")} \n";

    public override string Name => "Shaft Room";

    public override string AfterEnterLocation(IContext context, ILocation previousLocation)
    {
        var swordInPossession = context.HasItem<Sword>();

        if (swordInPossession)
            return "\nYour sword is glowing with a faint blue glow.";

        return string.Empty;
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        var basket = Repository.GetItem<Basket>();
        if (action.Match(["raise"], basket.NounsForMatching))
        {
            if (Items.Contains(basket))
                return new PositiveInteractionResult("Too late for that. ");

            ItemPlacedHere(basket);
            return new PositiveInteractionResult("The basket is raised to the top of the shaft. ");
        }

        if (action.Match(["lower"], basket.NounsForMatching))
        {
            if (!Items.Contains(basket))
                return new PositiveInteractionResult("Too late for that. ");

            Repository.GetLocation<DraftyRoom>().ItemPlacedHere(basket);
            return new PositiveInteractionResult("The basket is lowered to the bottom of the shaft. ");
        }

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override void Init()
    {
        StartWithItem<Chain>(this);
        StartWithItem<Basket>(this);
    }
}