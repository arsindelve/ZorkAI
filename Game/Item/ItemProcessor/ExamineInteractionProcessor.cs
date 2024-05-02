using Model.AIGeneration;
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
        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "examine":
            case "x":
            case "check":
            case "look":
            case "look at":
            case "peek at":
            case "look in":

                if (context is { HasLightSource: false, CurrentLocation: DarkLocation })
                    return new PositiveInteractionResult("It's too dark to see! ");

                if (item is ICanBeExamined castItemToExamine)
                    return new PositiveInteractionResult(castItemToExamine.ExaminationDescription);

                if (item is ItemBase castItem)
                    return new PositiveInteractionResult(
                        $"There is nothing special about the {castItem.NounsForMatching.MaxBy(s => s.Length)}. ");
                
                return null;
        }

        return null;
    }
}