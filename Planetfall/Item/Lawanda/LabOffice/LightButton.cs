using Model.AIGeneration;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.LabOffice;

public class LightButton : ItemBase
{
    public override string[] NounsForMatching => ["light button", "light"];

    public override int Size => 100; // Can't be taken

    public override Task<InteractionResult> RespondToSimpleInteraction(
        SimpleIntent action, IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(["push", "press", "activate"]))
            return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        var bioLab = Repository.GetLocation<BioLabLocation>();
        bioLab.LightsOn = true;

        return Task.FromResult<InteractionResult>(
            new PositiveInteractionResult("You push the LIGHT button. The Bio Lab lights turn on. "));
    }
}
