using Model.AIGeneration;

namespace Planetfall.Item.Computer;

public class Relay : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["relay", "microrelay", "micro-relay"];

    public string ExaminationDescription =>
        "This is a vacuum-sealed microrelay, encased in red translucent plastic. Within, you can see that some sort of speck " +
        "or impurity has wedged itself into the contact point of the relay, preventing it from closing. The speck, " +
        "presumably of microscopic size, resembles a blue boulder to you in your current size. ";

    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["look at", "look into"], NounsForMatching))
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(ExaminationDescription));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
