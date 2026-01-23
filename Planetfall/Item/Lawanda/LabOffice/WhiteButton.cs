using Model.AIGeneration;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.LabOffice;

public class WhiteButton : ItemBase
{
    public override string[] NounsForMatching => ["white button", "white", "button"];

    public override Task<InteractionResult?> RespondToSimpleInteraction(
        SimpleIntent action, IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchNounAndAdjective(NounsForMatching))
            return Task.FromResult<InteractionResult?>(new NoNounMatchInteractionResult());

        if (!action.MatchVerb(["push", "press", "activate"]))
            return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        return Task.FromResult<InteractionResult?>(
            new PositiveInteractionResult("You hear the faint sound of a relay clicking. "));
    }
}
