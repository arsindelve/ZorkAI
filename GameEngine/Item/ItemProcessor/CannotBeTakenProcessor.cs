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
        var verb = action.Verb.ToLowerInvariant().Trim();
        if (Verbs.TakeVerbs.Contains(verb) && !string.IsNullOrEmpty(castItem.CannotBeTakenDescription))
        {
            // Mirror TakeOrDropInteractionProcessor.TakeIt's refusal branch: fire the same
            // OnFailingToBeTaken hook, or a failed take's side effects (the destroy-on-failed-take
            // seam Slag/ToolChests use) would depend on which parse path delivered the verb.
            ((ItemBase)castItem).OnFailingToBeTaken(context);
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(castItem.CannotBeTakenDescription));
        }

        return Task.FromResult<InteractionResult?>(null);
    }
}