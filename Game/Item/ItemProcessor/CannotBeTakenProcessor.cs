using Model.Item;

namespace Game.Item.ItemProcessor;

public class CannotBeTakenProcessor : IVerbProcessor
{
    InteractionResult? IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item)
    {
        if (item is not IItem castItem) 
            throw new Exception("Cast Error");
        
        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "hold":
            case "take":
            case "pick up":
            case "acquire":
            case "snatch":
                return !string.IsNullOrEmpty(castItem.CannotBeTakenDescription) ? 
                    new PositiveInteractionResult(castItem.CannotBeTakenDescription) : 
                    null;
        }

        return null;
    }
}