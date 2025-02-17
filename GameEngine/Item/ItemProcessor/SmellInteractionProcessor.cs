using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item.ItemProcessor;

/// <summary>
///     Represents a processor for examining an item.
/// </summary>
public class SmellInteractionProcessor : IVerbProcessor
{
    Task<InteractionResult?> IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "smell":
            case "sniff":

                if (context.ItIsDarkHere)
                    return Task.FromResult<InteractionResult?>(new PositiveInteractionResult("It's too dark to see! "));

                if (item is ISmell castItemToSmell)
                    return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(castItemToSmell.SmellDescription));

                if (item is ItemBase castItem)
                    return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(
                        $"It smells like a {castItem.NounsForMatching.MaxBy(s => s.Length)}. "));

                return Task.FromResult<InteractionResult?>(null);
        }

        return Task.FromResult<InteractionResult?>(null);
    }
}