using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item.ItemProcessor;

public class ReadInteractionProcessor : IVerbProcessor
{
    public InteractionResult? Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        switch (action.Verb.ToLowerInvariant())
        {
            case "examine":
            case "look":
            case "look at":
            case "read":

                if (context.ItIsDarkHere)
                    return new PositiveInteractionResult("It's too dark to see!");

                var result = string.Empty;
                // The act of reading it picks it up.
                if (item is ICanBeTakenAndDropped && !context.Items.Contains((IItem)item))
                {
                    result = "(Taken)\n\n";
                    context.Take((IItem)item);
                }

                return new PositiveInteractionResult(result + ((ICanBeRead)item).ReadDescription);
        }

        return null;
    }
}