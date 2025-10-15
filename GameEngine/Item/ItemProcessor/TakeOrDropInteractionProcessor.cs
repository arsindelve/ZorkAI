using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
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
    async Task<InteractionResult?> IVerbProcessor.Process(SimpleIntent action, IContext context,
        IInteractionTarget item,
        IGenerationClient client)
    {
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
                return await GetItemsToTake(context, action);

            case "drop":
                return await GetItemsToDrop(context, action);
        }

        return null;
    }

    /// <summary>
    /// Processes a take interaction within the defined context.
    /// </summary>
    /// <param name="action">The take intent containing details of the interaction.</param>
    /// <param name="context">The context in which the take operation occurs.</param>
    /// <param name="client"></param>
    /// <returns>A tuple containing the interaction result and an associated message.</returns>
    public async Task<(InteractionResult? resultObject, string? ResultMessage)> Process(TakeIntent action,
        IContext context, IGenerationClient client)
    {
        var result = await GetItemsToTake(context,
            new SimpleIntent { OriginalInput = action.Message, Verb = "take", Noun = action.Noun });

        if (result is null or NoNounMatchInteractionResult)
        {
            var message = await client.GenerateNarration(
                new TakeSomethingThatIsNotPortable(action.OriginalInput), context.SystemPromptAddendum);
            return (new PositiveInteractionResult(message), message);
        }

        return (result, result.InteractionMessage);
    }

    public async Task<(InteractionResult? resultObject, string? ResultMessage)> Process(DropIntent action,
        IContext context, IGenerationClient client)
    {
        var result = await GetItemsToDrop(context,
            new SimpleIntent { OriginalInput = action.Message, Verb = "drop", Noun = action.Noun });

        if (result is null or NoNounMatchInteractionResult)
        {
            var message = await client.GenerateNarration(
                new DropSomethingTheyDoNotHave(action.OriginalInput), context.SystemPromptAddendum);
            return (new PositiveInteractionResult(message), message);
        }

        return (result, result.InteractionMessage);
    }

    private async Task<InteractionResult?> GetItemsToDrop(IContext context, SimpleIntent action)
    {
        if (string.IsNullOrEmpty(action.OriginalInput))
            return null;

        var items = await _itemParser.GetListOfItemsToDrop(action.OriginalInput,
            context.ItemListDescription(string.Empty + Environment.NewLine, null));

        // The parser did not see anything in the inventory that seemed like what we asked for
        if (!items.Any())
        {
            // There is still a chance there is something for us to drop. This can happen when the parser is not 
            // smart enough to match the noun to the item description. An example of this is the "magnet" which is 
            // (deliberately, as a puzzle) described as "a metal bar, curved into a U-shape" which the parser does not
            // understand is a magnet. So as a final attempt, let's see if there is a direct noun match.  
            var specificItem = Repository.GetItem(action.Noun);
            return specificItem is not null ? DropIt(context, specificItem) : new NoNounMatchInteractionResult();
        }

        if (items.Length == 1)
            return DropIt(context, Repository.GetItem(items[0]));

        return new PositiveInteractionResult(DropEverythingProcessor.DropAll(context,
            items.Select(Repository.GetItem).ToList()));
    }

    private async Task<InteractionResult?> GetItemsToTake(IContext context, SimpleIntent action)
    {
        if (string.IsNullOrEmpty(action.OriginalInput))
            return null;

        var items = await _itemParser.GetListOfItemsToTake(action.OriginalInput,
            context.CurrentLocation.GetDescriptionForGeneration(context));

        // The parser did not see anything in the room description that seemed like what we asked for
        if (!items.Any())
        {
            // There is still a chance there is something for us to pick up. This can happen when the parser is not 
            // smart enough to match the noun to the item description. An example of this is the "magnet" which is 
            // (deliberately, as a puzzle) described as "a metal bar, curved into a U-shape" which the parser does not
            // understand is a magnet. So as a final attempt, let's see if there is a direct noun match.  
            var specificItem = Repository.GetItem(action.Noun);
            return specificItem is not null ? TakeIt(context, specificItem) : new NoNounMatchInteractionResult();
        }

        if (items.Length == 1)
            return TakeIt(context, Repository.GetItem(items[0]));

        return new PositiveInteractionResult(TakeEverythingProcessor.TakeAll(context,
            items.Select(Repository.GetItem).ToList()));
    }

    public static InteractionResult DropIt(IContext context, IItem? castItem)
    {
        if (castItem is null) return new NoNounMatchInteractionResult();

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

        // This happens if you try to take something that is a legit object in the room,
        // but it has not been marked with this interface because it cannot be taken. Think doors and engravings. 
        if (castItem is not ICanBeTakenAndDropped takeItem)
            return new NoNounMatchInteractionResult();

        var onTakenText = takeItem.OnBeingTaken(context, container);
        container?.OnItemRemovedFromHere(castItem, context);

        return new PositiveInteractionResult(
            $"{(!string.IsNullOrEmpty(onTakenText) ? onTakenText + Environment.NewLine : "Taken. " )} ");
    }
}