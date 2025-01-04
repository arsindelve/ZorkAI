using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace GameEngine.Item.ItemProcessor;

public class TakeOrDropInteractionProcessor : IVerbProcessor
{
    InteractionResult? IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        if (item is not IItem castItem)
            throw new Exception();

        if (item is not ICanBeTakenAndDropped takeItem)
            throw new Exception();

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "hold":
            case "take":
            case "pick up":
            case "grab":
            case "get":
            case "acquire":
            case "snatch":
                return TakeIt(context, castItem, takeItem);

            case "drop":
                return DropIt(context, castItem);
        }

        return null;
    }

    public static InteractionResult DropIt(IContext context, IItem castItem)
    {
        if (!context.Items.Contains(castItem))
            return new PositiveInteractionResult("You don't have that!");

        if (castItem is IAmClothing { BeingWorn: true })
            return new PositiveInteractionResult("You'll have to take it off, first. ");

        if (context.CurrentLocation is IDropSpecialLocation specialLocation)
            return specialLocation.DropSpecial(castItem, context);

        context.Drop(castItem);
        return new PositiveInteractionResult("Dropped");
    }

    private static InteractionResult TakeIt(IContext context, IItem castItem,
        ICanBeTakenAndDropped takeItem)
    {
        if (!string.IsNullOrEmpty(castItem.CannotBeTakenDescription))
        {
            ((ItemBase)castItem).OnFailingToBeTaken(context);
            return new PositiveInteractionResult(castItem.CannotBeTakenDescription);
        }

        if (context is { HasLightSource: false, CurrentLocation: DarkLocation })
            return new PositiveInteractionResult("It's too dark to see!");

        if (context.Items.Contains(castItem))
            return new PositiveInteractionResult("You already have that!");

        var container = castItem.CurrentLocation;

        if (container is IOpenAndClose { IsOpen: false })
            return new PositiveInteractionResult("You can't reach something that's inside a closed container.");

        if (!context.HaveRoomForItem(castItem))
            return new PositiveInteractionResult("Your load is too heavy. ");

        context.Take(castItem);
        var onTakenText = takeItem.OnBeingTaken(context);
        container?.OnItemRemovedFromHere(castItem, context);

        return new PositiveInteractionResult(
            $"Taken. {(!string.IsNullOrEmpty(onTakenText) ? onTakenText + Environment.NewLine : string.Empty)} ");
    }
}