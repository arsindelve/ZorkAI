using Model.Item;

namespace Game.Item.ItemProcessor;

public class TakeOrDropInteractionProcessor : IVerbProcessor
{
    InteractionResult? IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item)
    {
        if (item is not IItem castItem)
            throw new Exception();

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "hold":
            case "take":
            case "pick up":
            case "acquire":
            case "snatch":
                context.Take(castItem);
                return new PositiveInteractionResult("Taken.");

            case "drop":
                context.Drop(castItem);
                return new PositiveInteractionResult("Dropped");
        }

        return null;
    }
}