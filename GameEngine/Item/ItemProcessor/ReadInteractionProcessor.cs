using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item.ItemProcessor;

public class ReadInteractionProcessor : IVerbProcessor
{
    // The verbs anything readable answers. Declared here because this processor is what answers them.
    private static readonly VerbFamily ReadVerbs = new("read", "examine", "look", "look at");

    public Task<InteractionResult?> Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        switch (action.Verb.ToLowerInvariant())
        {
            case var verb when ReadVerbs.Contains(verb):

                if (context.ItIsDarkHere)
                    return Task.FromResult<InteractionResult?>(new PositiveInteractionResult("It's too dark to see!"));

                var result = string.Empty;
                // The act of reading it picks it up.
                if (item is ICanBeTakenAndDropped && !context.Items.Contains((IItem)item))
                {
                    result = "(Taken)\n\n";
                    context.Take((IItem)item);
                }

                return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(result + ((ICanBeRead)item).ReadDescription));
        }

        return Task.FromResult<InteractionResult?>(null);
    }
}