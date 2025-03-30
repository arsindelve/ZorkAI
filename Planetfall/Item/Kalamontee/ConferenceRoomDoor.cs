using Model.AIGeneration;

namespace Planetfall.Item.Kalamontee;

public class ConferenceRoomDoor : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForMatching => ["conference room door", "door"];

    [UsedImplicitly]
    public string Code { get; set; } = "0";

    [UsedImplicitly]
    public string UnlockCode { get; set; } = new Random().Next(1, 999).ToString();
    public bool IsOpen { get; set; }
    public string AlreadyOpen => "It's already open! ";
    public string AlreadyClosed => "It is closed! ";
    public bool HasEverBeenOpened { get; set; }

    public string NowOpen(ILocation currentLocation)
    {
        // The door can never be opened directly. It opens automatically when the dial has the correct code. 
        throw new NotImplementedException();
    }

    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["turn", "set"], ["dial"]))
            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult("You must specify a number to set the dial to. "));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        if (context.CurrentLocation is not RecArea)
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.MatchVerb(["turn", "set"]))
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.MatchPreposition(["to"]))
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.MatchNounOne(["dial", "door", "conference room door", "lock"]))
            return base.RespondToMultiNounInteraction(action, context);

        return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(AttemptUnlock(action.NounTwo)));
    }

    private string AttemptUnlock(string actionNounTwo)
    {
        var result = AnalyzeDialInput(actionNounTwo);

        switch (result)
        {
            case TurnDialResult.IsNumberBetween0And999:
            {
                if (actionNounTwo != UnlockCode)
                {
                    Code = actionNounTwo;
                    return $"The dial is now set to {actionNounTwo}. ";
                }

                IsOpen = true;
                Code = "0";
                return "The door swings open, and the dial resets to 0. ";
            }
            case TurnDialResult.IsNumberAbove999:
                return "The dial does not go that high. ";
            case TurnDialResult.IsNumberBelow0:
                return "The numbers on the dial do not go below zero. ";
            case TurnDialResult.IsNotANumber:
                return "The dial can only be set to numbers. ";
        }

        throw new ArgumentOutOfRangeException();
    }

    public string NowClosed(ILocation currentLocation)
    {
        Code = "0";
        return "The door closes and you hear a click as it locks. ";
    }

    public string CannotBeOpenedDescription(IContext context)
    {
        return context.CurrentLocation is ConferenceRoom
            ? "The door seems to be locked from the other side."
            : "The door is locked. You probably have to turn the dial to some number to open it. ";
    }

    public string ExaminationDescription => $"The door is {(IsOpen ? "open" : "closed")}. ";

    private static TurnDialResult AnalyzeDialInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return TurnDialResult.IsNotANumber;
        }

        if (int.TryParse(input, out int result))
        {
            return result switch
            {
                >= 0 and <= 999 => TurnDialResult.IsNumberBetween0And999,
                > 999 => TurnDialResult.IsNumberAbove999,
                _ => TurnDialResult.IsNumberBelow0
            };
        }

        return TurnDialResult.IsNotANumber;
    }

    private enum TurnDialResult
    {
        IsNumberBetween0And999,
        IsNumberAbove999,
        IsNumberBelow0,
        IsNotANumber
    }
}