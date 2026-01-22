using Model.AIGeneration;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.LabOffice;

public class RedButton : ItemBase
{
    public override string[] NounsForMatching => ["red button", "red", "button"];

    public override Task<InteractionResult?> RespondToSimpleInteraction(
        SimpleIntent action, IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(["push", "press", "activate"]))
            return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        var timer = Repository.GetItem<FungicideTimer>();

        // Add timer to actors if not already there
        if (!context.Actors.Contains(timer))
            context.RegisterActor(timer);

        timer.Reset();

        return Task.FromResult<InteractionResult?>(
            new PositiveInteractionResult("You hear a hissing from beyond the door to the west. "));
    }
}
