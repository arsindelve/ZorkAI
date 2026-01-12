using Model.AIGeneration;

namespace Planetfall.Item.Kalamontee.Mech;

public class Laser : ContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["laser", "portable laser"];

    public override Type[] CanOnlyHoldTheseTypes => [typeof(BatteryBase)];
    
    public override int Size => 1;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a laser here. ";
    }

    private readonly Dictionary<int, string> _dialColors = new()
    {
        { 1, "red" },
        { 2, "orange" },
        { 3, "yellow" },
        { 4, "green" },
        { 5, "blue" },
        { 6, "violet" }
    };
    
    public int Setting { get; set; } = 5;

    public string ExaminationDescription =>
        "The laser, though portable, is still fairly heavy. It has a long, slender " +
        "barrel and a dial with six settings, labelled \"1\" through \"6.\" This " +
        $"dial is currently on setting {Setting}. There is a depression on the top of the " +
        "laser which contains an old battery.";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "A small device, labelled \"Akmee Portabul Laazur\", is resting on one of the lower shelves. ";
    }

    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["turn", "set"], ["dial", "laser", "setting"]))
            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult("You must specify a number to set the dial to. "));

        if (action.Match(["shoot", "fire", "activate", "discharge"], NounsForMatching))
            return Task.FromResult<InteractionResult?>(
                ShootLaser(context));
    
        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    private InteractionResult ShootLaser(IContext context)
    {
        if (CurrentLocation != context)
            return new PositiveInteractionResult("You're not holding the laser. ");
        
        BatteryBase? battery = Items.FirstOrDefault() as BatteryBase;
        int chargesRemaining = battery?.ChargesRemaining ?? 0;

        if (chargesRemaining == 0)
        {
            return new PositiveInteractionResult("Click.");
        }
        
        battery!.ChargesRemaining--;
        return new PositiveInteractionResult($"The laser emits a narrow {_dialColors[Setting]} beam of light. ");
    }

    public override Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        if (!action.MatchVerb(["turn", "set"]))
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.MatchPreposition(["to"]))
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.MatchNounOne(["dial", "laser", "setting"]))
            return base.RespondToMultiNounInteraction(action, context);

        return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(AttemptUnlock(action.NounTwo)));
    }
    
    private string AttemptUnlock(string actionNounTwo)
    {
        var result = AnalyzeDialInput(actionNounTwo);

        switch (result.resultType)
        {
            case TurnDialResult.IsNumberBetween1And6:
            {
                if (result.number == Setting)
                    return "That's where it's set now!";

                Setting = result.number.GetValueOrDefault();
                return $"The dial is now set to {Setting}. ";
            }
            case TurnDialResult.IsNotANumber:
            case TurnDialResult.IsNumberAbove6:
            case TurnDialResult.IsNumberBelow1:
                return "The dial can only be set to numbers between 1 and 6";
        }

        throw new ArgumentOutOfRangeException();
    }
    
    private static (TurnDialResult resultType, int? number) AnalyzeDialInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (TurnDialResult.IsNotANumber, null);
        }

        if (int.TryParse(input, out int result))
        {
            return result switch
            {
                >= 1 and <= 6 => (TurnDialResult.IsNumberBetween1And6, result),
                > 6 => (TurnDialResult.IsNumberAbove6, null),
                _ => (TurnDialResult.IsNumberBelow1, null)
            };
        }

        return (TurnDialResult.IsNotANumber, null);
    }

    private enum TurnDialResult
    {
        IsNumberBetween1And6,
        IsNumberAbove6,
        IsNumberBelow1,
        IsNotANumber
    }
    
    public override void Init()
    {
        StartWithItemInside<OldBattery>();
    }
}