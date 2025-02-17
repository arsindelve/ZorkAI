using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item.ItemProcessor;

public class OpenAndCloseInteractionProcessor : IVerbProcessor
{
    Task<InteractionResult?> IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        if (item is not IOpenAndClose castItem)
            throw new Exception("Cast Error");

        if (Verbs.OpenVerbs.Contains(action.Verb.ToLowerInvariant().Trim()))
            return Task.FromResult<InteractionResult?>(OpenMe(castItem, context));

        if (Verbs.CloseVerbs.Contains(action.Verb.ToLowerInvariant().Trim()))
            return Task.FromResult<InteractionResult?>(CloseMe(castItem, context));

        return Task.FromResult<InteractionResult?>(null);
    }

    private InteractionResult OpenMe(IOpenAndClose item, IContext context)
    {
        if (item.IsOpen)
            return new PositiveInteractionResult(item.AlreadyOpen);

        // A non-empty, non-null string here indicates the item cannot be opened, and why. 
        var cannotBeOpenedReason = item.CannotBeOpenedDescription(context);
        if (!string.IsNullOrEmpty(cannotBeOpenedReason))
            return new PositiveInteractionResult(cannotBeOpenedReason);

        var returnText = item.NowOpen(context.CurrentLocation);

        item.IsOpen = true;
        item.HasEverBeenOpened = true;
        returnText += item.OnOpening(context);

        return new PositiveInteractionResult(returnText);
    }

    private InteractionResult CloseMe(IOpenAndClose item, IContext context)
    {
        if (!item.IsOpen)
            return new PositiveInteractionResult(item.AlreadyClosed);

        // A non-empty, non-null string here indicates the item cannot be closed, and why. 
        var cannotBeOpenedReason = item.CannotBeClosedDescription(context);
        if (!string.IsNullOrEmpty(cannotBeOpenedReason))
            return new PositiveInteractionResult(cannotBeOpenedReason);

        item.IsOpen = false;
        return new PositiveInteractionResult(item.NowClosed(context.CurrentLocation));
    }
}