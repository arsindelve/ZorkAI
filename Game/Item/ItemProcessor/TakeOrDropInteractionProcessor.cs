using Model.Item;

namespace Game.Item.ItemProcessor;

public class TakeOrDropInteractionProcessor : IVerbProcessor
{
    InteractionResult? IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item)
    {
        if (item is not IItem castItem)
            throw new Exception();

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "hold":
            case "take":
            case "pick up":
            case "acquire":
            case "snatch":
                if (context.HasMatchingNoun(action.Noun))
                    return new PositiveInteractionResult("You already have that!");

                context.Take(castItem);
                return new PositiveInteractionResult("Taken.");

            case "drop":
                if (!context.HasMatchingNoun(action.Noun))
                    return new PositiveInteractionResult("You don't have that!");

                context.Drop(castItem);
                return new PositiveInteractionResult("Dropped");
        }

        return null;
    }
}