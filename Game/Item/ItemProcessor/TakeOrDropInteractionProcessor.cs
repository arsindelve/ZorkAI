using Model.Item;

namespace Game.Item.ItemProcessor;

public class TakeOrDropInteractionProcessor : IVerbProcessor
{
    InteractionResult? IVerbProcessor.Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        if (item is not IItem castItem)
            throw new Exception();

        if (item is not ICanBeTakenAndDropped takeItem)
            throw new Exception();

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case "hold":
            case "take":
            case "pick up":
            case "acquire":
            case "snatch":

                if (!string.IsNullOrEmpty(castItem.CannotBeTakenDescription))
                    return new PositiveInteractionResult(castItem.CannotBeTakenDescription);

                if (context is { HasLightSource: false, CurrentLocation: DarkLocation })
                    return new PositiveInteractionResult("It's too dark to see!");

                if (context.HasMatchingNoun(action.Noun, false))
                    // We have to take it from the container. 
                    return new PositiveInteractionResult("You already have that!");

                var container = castItem.CurrentLocation;

                if (container is IOpenAndClose { IsOpen: false })
                    return new PositiveInteractionResult("You can't reach something that's inside a closed container.");

                context.Take(castItem);
                takeItem.OnBeingTaken(context);
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