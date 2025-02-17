using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item.ItemProcessor;

public class CannotBeTakenProcessor : IVerbProcessor
{
    Task<InteractionResult?> IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
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
                return !string.IsNullOrEmpty(castItem.CannotBeTakenDescription)
                    ? Task.FromResult<InteractionResult?>(new PositiveInteractionResult(castItem.CannotBeTakenDescription))
                    : Task.FromResult<InteractionResult?>(null);
        }

        return Task.FromResult<InteractionResult?>(null);
    }
}