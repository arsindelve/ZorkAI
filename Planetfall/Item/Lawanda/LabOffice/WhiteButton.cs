using Model.AIGeneration;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.LabOffice;

public class WhiteButton : ItemBase
{
    public override string[] NounsForMatching => ["button", "white button"];

    public override Task<InteractionResult?> RespondToSimpleInteraction(
        SimpleIntent action, IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(["push", "press", "activate"]))
            return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        var bioLab = Repository.GetLocation<BioLabLocation>();
        bioLab.LightsOn = true;

        return Task.FromResult<InteractionResult?>(
            new PositiveInteractionResult("You hear the faint sound of a relay clicking. "));
    }
}
