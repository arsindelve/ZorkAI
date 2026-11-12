using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item.ItemProcessor;

/// <summary>
///     Represents a processor for examining an item.
/// </summary>
public class ExamineInteractionProcessor : IVerbProcessor
{
    // What this processor itself answers. Not the same set as Verbs.ExamineVerbs, which is the wider
    // family rooms and items match on - kept word for word as it was, so nothing changes behaviour.
    private static readonly VerbFamily AnsweredVerbs =
        new("examine", "x", "check", "look", "look at", "peek at", "look in");

    Task<InteractionResult?> IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case var verb when AnsweredVerbs.Contains(verb):

                if (context is { HasLightSource: false, CurrentLocation: DarkLocation })
                    return Task.FromResult<InteractionResult?>(new PositiveInteractionResult("It's too dark to see! "));

                if (item is ICanBeExamined castItemToExamine)
                {
                    castItemToExamine.OnBeingExamined(context);
                    return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(castItemToExamine.ExaminationDescription));
                }

                if (item is ItemBase castItem)
                    return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(
                        $"There is nothing special about the {castItem.NounsForMatching.MaxBy(s => s.Length)}. "));

                return Task.FromResult<InteractionResult?>(null);
        }

        return Task.FromResult<InteractionResult?>(null);
    }
}