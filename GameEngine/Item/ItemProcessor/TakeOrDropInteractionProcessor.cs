using Model.AIGeneration;
using Model.AIParsing;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace GameEngine.Item.ItemProcessor;

/// <summary>
/// The TakeOrDropInteractionProcessor class is responsible for managing interactions
/// related to taking or dropping items within the game. It implements the IVerbProcessor
/// interface to process specific user actions.
/// This class provides functionality to handle the logic for dropping an item from the context,
/// considering various conditions such as whether the item is worn or if it requires special
/// handling based on the current location.
/// </summary>
public class TakeOrDropInteractionProcessor : IVerbProcessor
{
    private readonly IAITakeAndAndDropParser _itemParser;

    /// <summary>
    /// Processes interactions related to taking or dropping items in a gaming context.
    /// </summary>
    public TakeOrDropInteractionProcessor(IAITakeAndAndDropParser itemParser)
    {
        _itemParser = itemParser;
    }

    /// <summary>
    /// Processes interaction verbs, such as taking or dropping an item, based on the provided context and parameters.
    /// </summary>
    /// <param name="action">The action that contains the interaction verb to be processed.</param>
    /// <param name="context">The context in which the interaction takes place, including information about the current state of the game or system.</param>
    /// <param name="item">The target item that the action is being performed on.</param>
    /// <param name="client">The generation client used for any additional logic or AI-driven interactions required during processing.</param>
    /// <returns>An <see cref="InteractionResult"/> indicating the outcome of the interaction, or null if no valid action was processed.</returns>
    /// <exception cref="Exception">Thrown when the item does not implement required interfaces for the action.</exception>
    Task<InteractionResult?> IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        if (item is not IItem castItem)
            throw new Exception();

        if (item is not ICanBeTakenAndDropped)
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
                return Task.FromResult<InteractionResult?>(TakeIt(context, castItem));

            case "drop":
                return Task.FromResult<InteractionResult?>(DropIt(context, castItem));
        }

        return Task.FromResult<InteractionResult?>(null);
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

    public static InteractionResult TakeIt(IContext context, IItem? castItem)
    {
        if (castItem is null)
            return new NoNounMatchInteractionResult();
        
        if (!string.IsNullOrEmpty(castItem.CannotBeTakenDescription))
        {
            ((ItemBase)castItem).OnFailingToBeTaken(context);
            return new PositiveInteractionResult(castItem.CannotBeTakenDescription);
        }

        if (context.Items.Contains(castItem))
            return new PositiveInteractionResult("You already have that!");

        var container = castItem.CurrentLocation;

        if (container is IOpenAndClose { IsOpen: false })
            return new PositiveInteractionResult("You can't reach something that's inside a closed container.");

        if (!context.HaveRoomForItem(castItem))
            return new PositiveInteractionResult("Your load is too heavy. ");

        context.Take(castItem);

        var takeItem = (ICanBeTakenAndDropped)castItem;
        var onTakenText = takeItem.OnBeingTaken(context);
        container?.OnItemRemovedFromHere(castItem, context);

        return new PositiveInteractionResult(
            $"Taken. {(!string.IsNullOrEmpty(onTakenText) ? onTakenText + Environment.NewLine : string.Empty)} ");
    }
}