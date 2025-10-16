using Model.AIGeneration;

namespace Planetfall.Item.Lawanda.PlanetaryDefense;

public abstract class FromitzBoardBase : ItemBase, ICanBeExamined, ICanBeTakenAndDropped
{
    /// <summary>
    /// Remove "board" and "fromitz board" from the list of disambiguation nouns. The adventurer will have to be more specific. 
    /// </summary>
    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["card", "access card"]).ToArray();

    public override int Size => 1;

    public string ExaminationDescription =>
        "Like most fromitz boards, it is a twisted maze of silicon circuits. It is square, approximately seventeen centimeters on each side. ";

    public override string? CannotBeTakenDescription =>
        "You jerk your hand back as you receive a powerful shock from the fromitz board. ";

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // Redirect "remove" verb to the Take processor only when the board is inside the FromitzAccessPanel
        if (!action.MatchVerb(["remove"]) ||
            !action.MatchNoun(NounsForMatching) ||
            CurrentLocation is not FromitzAccessPanel)
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

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public virtual string OnTheGroundDescription(ILocation currentLocation)
    {
        return string.Empty;
    }
}