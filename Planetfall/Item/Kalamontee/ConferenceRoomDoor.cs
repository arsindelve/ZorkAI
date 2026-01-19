using GameEngine;
using Model.AIGeneration;
using Model.Interface;
using Newtonsoft.Json;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Utilities;

namespace Planetfall.Item.Kalamontee;

public class ConferenceRoomDoor : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForMatching => ["conference room door", "door"];

    [UsedImplicitly] [JsonIgnore]
    public IRandomChooser Chooser { get; set; } = new RandomChooser();

    [UsedImplicitly]
    public string Code { get; set; } = "0";

    private string? _unlockCode;

    [UsedImplicitly]
    public string UnlockCode
    {
        get => _unlockCode ??= Chooser.RollDice(999).ToString();
        set => _unlockCode = value;
    }

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
        {
            // Try to extract text after "to" from the original input (handles "set dial to twelve" case)
            var textAfterTo = action.OriginalInput.ExtractTextAfterTo();
            if (textAfterTo != null && context.CurrentLocation is RecArea)
                return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(AttemptUnlock(textAfterTo, context)));

            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult("You must specify a number to set the dial to. "));
        }

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

        return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(AttemptUnlock(action.NounTwo, context)));
    }

    private string AttemptUnlock(string actionNounTwo, IContext context)
    {
        var result = AnalyzeDialInput(actionNounTwo);

        switch (result.resultType)
        {
            case TurnDialResult.IsNumberBetween0And999:
            {
                var parsedNumber = result.number!.Value;
                var unlockCodeNumber = int.Parse(UnlockCode);

                if (parsedNumber != unlockCodeNumber)
                {
                    Code = parsedNumber.ToString();
                    return $"The dial is now set to {parsedNumber}. ";
                }

                IsOpen = true;
                Code = "0";
                Repository.GetItem<Floyd>().CommentOnAction(FloydPrompts.ConferenceRoomDoorOpened, context);
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

    private static (TurnDialResult resultType, int? number) AnalyzeDialInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (TurnDialResult.IsNotANumber, null);
        }

        // Use ToInteger() to support both numeric ("12") and word ("twelve") input
        var result = input.ToInteger();

        if (result.HasValue)
        {
            return result.Value switch
            {
                >= 0 and <= 999 => (TurnDialResult.IsNumberBetween0And999, result.Value),
                > 999 => (TurnDialResult.IsNumberAbove999, null),
                _ => (TurnDialResult.IsNumberBelow0, null)
            };
        }

        return (TurnDialResult.IsNotANumber, null);
    }

    private enum TurnDialResult
    {
        IsNumberBetween0And999,
        IsNumberAbove999,
        IsNumberBelow0,
        IsNotANumber
    }
}