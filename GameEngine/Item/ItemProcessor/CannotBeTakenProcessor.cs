using Model;
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

        // Consult the canonical take-verb family, not a local copy: a hardcoded list here had
        // drifted from Verbs.TakeVerbs (it was missing "get"/"grab"), so those phrasings skipped the
        // item's authored CannotBeTakenDescription and fell through to the improvised "verb has no
        // effect" narration (issue #406).
        if (Verbs.TakeVerbs.Contains(action.Verb.ToLowerInvariant().Trim()))
            return !string.IsNullOrEmpty(castItem.CannotBeTakenDescription)
                ? Task.FromResult<InteractionResult?>(new PositiveInteractionResult(castItem.CannotBeTakenDescription))
                : Task.FromResult<InteractionResult?>(null);

        return Task.FromResult<InteractionResult?>(null);
    }
}