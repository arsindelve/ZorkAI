using Model.Item;

namespace Game.Item.ItemProcessor;

public class OpenAndCloseInteractionProcessor : IVerbProcessor
{
    InteractionResult? IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        if (item is not IOpenAndClose castItem)
            throw new Exception("Cast Error");

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "open":
                return OpenMe(castItem);

            case "close":
            case "shut":
                return CloseMe(castItem);
        }

        return null;
    }

    private InteractionResult OpenMe(IOpenAndClose item)
    {
        if (item.IsOpen)
            return new PositiveInteractionResult(item.AlreadyOpen);

        var returnText = item.NowOpen;

        item.IsOpen = true;
        item.HasEverBeenOpened = true;

        return new PositiveInteractionResult(returnText);
    }

    private InteractionResult CloseMe(IOpenAndClose item)
    {
        if (!item.IsOpen)
            return new PositiveInteractionResult(item.AlreadyClosed);

        item.IsOpen = false;
        return new PositiveInteractionResult(item.NowClosed);
    }
}