using Model.Item;

namespace Game.Item.ItemProcessor;

/// <summary>
///     Represents a processor for examining an item.
/// </summary>
public class ExamineInteractionProcessor : IVerbProcessor
{
    InteractionResult? IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        if (item is not ICanBeExamined castItem)
            throw new Exception("Cast Error");

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "examine":
            case "check":
            case "look":
            case "look at":
            case "peek at":
            case "look in":

                if (context is { HasLightSource: false, CurrentLocation: DarkLocation })
                    return new PositiveInteractionResult("It's too dark to see!");

                return new PositiveInteractionResult(castItem.ExaminationDescription);
        }

        return null;
    }
}