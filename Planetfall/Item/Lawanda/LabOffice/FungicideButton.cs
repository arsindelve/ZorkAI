using Model.AIGeneration;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.LabOffice;

public class FungicideButton : ItemBase
{
    public override string[] NounsForMatching => ["fungicide button", "fungicide", "button"];

    public override int Size => 100; // Can't be taken

    public override Task<InteractionResult> RespondToSimpleInteraction(
        SimpleIntent action, IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(["push", "press", "activate"]))
            return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        var timer = Repository.GetItem<FungicideTimer>();

        // Add timer to actors if not already there
        if (!context.Actors.Contains(timer))
            context.RegisterActor(timer);

        timer.Reset();

        return Task.FromResult<InteractionResult>(
            new PositiveInteractionResult(
                "You push the FUNGICIDE button. A misting system activates in the Bio Lab with a soft hiss. " +
                "The fungicide should protect against the mutants for approximately 50 turns. "));
    }
}
