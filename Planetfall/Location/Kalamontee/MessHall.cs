using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee;

internal class MessHall : LocationBase
{
    public override string Name => "Mess Hall";

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["look"], ["table"]) && action.OriginalInput != null && action.OriginalInput.Contains("under"))
            return new PositiveInteractionResult(
                "Wow!!! Under the table are three keys, a sack of food, a reactor elevator access pass, just kidding. Actually, there's nothing there.");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<MessCorridor>() },
            {
                Direction.S, new MovementParameters
                {
                    CanGo = _ => GetItem<KitchenDoor>().IsOpen,
                    Location = GetLocation<Kitchen>(),
                    CustomFailureMessage = "The door is closed. "
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a large hall lined with tables and benches. An opening to the north leads back to the corridor. " +
            "A door to the south is closed. Next to the door is a small slot. ";
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        if (previousLocation is Kitchen)
        {
            var door = GetItem<KitchenDoor>();
            door.IsOpen = false;
            return Task.FromResult(door.NowClosed(this));
        }

        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override void Init()
    {
        StartWithItem<Canteen>();
        StartWithItem<KitchenDoor>();
        StartWithItem<KitchenSlot>();
    }
}