using Model.AIGeneration;

namespace Planetfall.Item.Lawanda.LabOffice;

public class OfficeDoor : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["door", "office door"];

    public override int Size => 100; // Can't be taken

    [UsedImplicitly]
    public bool IsOpen { get; set; } = false;

    public string ExaminationDescription =>
        IsOpen
            ? "The office door is open, leading west to the Bio Lab. "
            : "The office door is closed. ";

    public override Task<InteractionResult> RespondToSimpleInteraction(
        SimpleIntent action, IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.MatchVerb(["open"]))
        {
            if (IsOpen)
                return Task.FromResult<InteractionResult>(
                    new PositiveInteractionResult("The door is already open. "));

            IsOpen = true;
            return Task.FromResult<InteractionResult>(
                new PositiveInteractionResult("You open the office door. "));
        }

        if (action.MatchVerb(["close"]))
        {
            if (!IsOpen)
                return Task.FromResult<InteractionResult>(
                    new PositiveInteractionResult("The door is already closed. "));

            IsOpen = false;
            return Task.FromResult<InteractionResult>(
                new PositiveInteractionResult("You close the office door. "));
        }

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
