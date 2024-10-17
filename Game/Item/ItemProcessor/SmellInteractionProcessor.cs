using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace Game.Item.ItemProcessor;

/// <summary>
///     Represents a processor for examining an item.
/// </summary>
public class SmellInteractionProcessor : IVerbProcessor
{
    InteractionResult? IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "smell":
            case "sniff":

                if (context.ItIsDarkHere)
                    return new PositiveInteractionResult("It's too dark to see! ");

                if (item is ISmell castItemToSmell)
                    return new PositiveInteractionResult(castItemToSmell.SmellDescription);

                if (item is ItemBase castItem)
                    return new PositiveInteractionResult(
                        $"It smells like a {castItem.NounsForMatching.MaxBy(s => s.Length)}. ");

                return null;
        }

        return null;
    }
}