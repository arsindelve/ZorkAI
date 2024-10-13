using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item.ItemProcessor;

public class OpenAndCloseInteractionProcessor : IVerbProcessor
{
    InteractionResult? IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        if (item is not IOpenAndClose castItem)
            throw new Exception("Cast Error");

        if (Verbs.OpenVerbs.Contains(action.Verb.ToLowerInvariant().Trim()))
            return OpenMe(castItem, context);
        
        if (Verbs.CloseVerbs.Contains(action.Verb.ToLowerInvariant().Trim())) 
            return CloseMe(castItem, context);

        return null;
    }

    private InteractionResult OpenMe(IOpenAndClose item, IContext context)
    {
        if (item.IsOpen)
            return new PositiveInteractionResult(item.AlreadyOpen);

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

        item.IsOpen = false;
        return new PositiveInteractionResult(item.NowClosed(context.CurrentLocation));
    }
}