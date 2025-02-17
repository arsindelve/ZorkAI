using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class ShaftRoom : DarkLocation, IThiefMayVisit
{
    public override string Name => "Shaft Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
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
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a large room, in the middle of which is a small shaft descending through the floor into darkness " +
            "below. To the west and the north are exits from this room. Constructed over the top of the shaft is a metal " +
            $"framework to which a heavy iron chain is attached. {(Items.Contains(Repository.GetItem<Basket>()) ? "\nAt the end of the chain is a basket. "
                + (GetItem<Basket>().Items.Any() ? GetItem<Basket>().ItemListDescription("basket", null) : "") : "")} \n";
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient? generationClient)
    {
        var swordInPossession = context.HasItem<Sword>();

        if (swordInPossession)
            return Task.FromResult("\nYour sword is glowing with a faint blue glow.");

        return Task.FromResult(string.Empty);
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
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

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override void Init()
    {
        StartWithItem<Chain>();
        StartWithItem<Basket>();
    }
}