using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Movement;

namespace ZorkOne.Location;

public class BehindHouse : LocationBase
{
    public override string Name => "Behind House";

    protected override string GetContextBasedDescription(IContext context)
    {
        return $"You are behind the white house. A path leads into the forest to the east. In one corner " +
               $"of the house there is a small window which is {(Repository.GetItem<KitchenWindow>().IsOpen ? "open" : "slightly ajar")}. ";
    }

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        // Handle any "through window" command
        if (input != null && input.ToLowerInvariant().Contains("through") &&
            input.ToLowerInvariant().Contains("window"))
        {
            var window = Repository.GetItem<KitchenWindow>();
            if (window.IsOpen)
            {
                return new PositiveInteractionResult("The window is open. If you want to enter the house, just say so. ");
            }
            else
            {
                return new PositiveInteractionResult("The window is slightly ajar, but not enough to permit entry. ");
            }
        }

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        MovementParameters enterKitchen = new()
        {
            CanGo = _ => Repository.GetItem<KitchenWindow>().IsOpen,
            CustomFailureMessage = "The kitchen window is closed. ",
            Location = Repository.GetLocation<Kitchen>()
        };

        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.E, new MovementParameters { Location = GetLocation<ClearingBehindHouse>() }
            },
            {
                Direction.S, new MovementParameters { Location = GetLocation<SouthOfHouse>() }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<NorthOfHouse>() }
            },
            {
                Direction.W, enterKitchen
            },
            {
                Direction.In, enterKitchen
            }
        };
    }

    public override void Init()
    {
        StartWithItem<KitchenWindow>();
    }
}