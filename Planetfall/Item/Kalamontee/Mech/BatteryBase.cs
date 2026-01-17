using Model.AIGeneration;

namespace Planetfall.Item.Kalamontee.Mech;

public abstract class BatteryBase : ItemBase
{
    public override int Size => 1;

    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["battery"]).ToArray();

    public abstract int ChargesRemaining { get; set; }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // Redirect "remove" verb to the Take processor only when the battery is inside the Laser
        if (!action.MatchVerb(["remove"]) ||
            !action.MatchNoun(NounsForMatching) ||
            CurrentLocation is not Laser)
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        var processors = itemProcessorFactory.GetProcessors(this);
        var takeProcessor = processors.FirstOrDefault(p => p.GetType().Name == "TakeOrDropInteractionProcessor");

        if (takeProcessor == null)
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        var takeIntent = new SimpleIntent
        {
            Verb = "take",
            Noun = action.Noun,
            OriginalInput = action.OriginalInput
        };

        return await takeProcessor.Process(takeIntent, context, this, client);
    }
}