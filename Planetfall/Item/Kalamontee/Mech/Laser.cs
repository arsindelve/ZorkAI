using Model.AIGeneration;
using Newtonsoft.Json;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Utilities;

namespace Planetfall.Item.Kalamontee.Mech;

public class Laser : ContainerBase, ICanBeTakenAndDropped, ICanBeExamined, ITurnBasedActor
{
    [UsedImplicitly] [JsonIgnore]
    public IRandomChooser Chooser { get; set; } = new RandomChooser();

    /// <summary>
    /// Tracks the laser's current temperature level (0 = cold, increases with each shot).
    /// </summary>
    public int WarmthLevel { get; set; }

    public override bool IsTransparent => true;

    /// <summary>
    /// Flag indicating whether the laser was just shot this turn.
    /// Reset at the end of each turn's Act call.
    /// </summary>
    public bool JustShot { get; set; }

    /// <summary>
    /// Flag indicating whether the laser has ever been successfully fired.
    /// Used for awarding points on first successful shot.
    /// </summary>
    [UsedImplicitly]
    public bool HasBeenFired { get; set; }

    public override string[] NounsForMatching =>
        ["laser", "portable laser", "akmee portabul laazur", "laazur", "akmee laazur"];

    public override Type[] CanOnlyHoldTheseTypes => [typeof(BatteryBase)];

    public override int Size => 1;

    public override string ItemPlacedHereResult(IItem item, IContext context)
    {
        return "The battery is now resting in the depression, attached to the laser. ";
    }

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
        {
            // Try to extract text after "to" from the original input (handles "set laser to one" or "set laser to blue" case)
            var textAfterTo = action.OriginalInput.ExtractTextAfterTo();
            if (textAfterTo != null)
                return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(AttemptSetDial(textAfterTo)));

            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult("You must specify a number to set the dial to. "));
        }

        if (action.Match(["shoot", "fire", "activate", "discharge"], NounsForMatching))
            return Task.FromResult<InteractionResult?>(
                ShootLaser(context));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    /// <summary>
    /// Attempts to fire the laser. Returns an error result if firing fails, or null if successful.
    /// On success, decrements battery charge and registers the laser as an actor for warmth tracking.
    /// </summary>
    private InteractionResult? TryFireLaser(IContext context)
    {
        if (CurrentLocation != context)
            return new PositiveInteractionResult("You're not holding the laser. ");

        BatteryBase? battery = Items.FirstOrDefault() as BatteryBase;
        int chargesRemaining = battery?.ChargesRemaining ?? 0;

        if (chargesRemaining == 0)
            return new PositiveInteractionResult("Click.");

        battery!.ChargesRemaining--;
        JustShot = true;
        context.RegisterActor(this);

        if (!HasBeenFired)
        {
            HasBeenFired = true;
            context.AddPoints(2);
        }

        return null; // Success - laser fired
    }

    private string BeamDescription => $"The laser emits a narrow {_dialColors[Setting]} beam of light";

    private InteractionResult ShootLaser(IContext context)
    {
        var fireResult = TryFireLaser(context);
        if (fireResult != null)
            return fireResult;

        return new PositiveInteractionResult($"{BeamDescription}. ");
    }

    private InteractionResult ShootLaserAt(string targetNoun, IContext context)
    {
        // Check target scope before firing (don't waste a charge on invalid target)
        var target = Repository.GetItemInScope(targetNoun, context);
        if (target == null)
            return new PositiveInteractionResult($"You don't see any {targetNoun} here. ");

        var fireResult = TryFireLaser(context);
        if (fireResult != null)
            return fireResult;

        // Special response for shooting Floyd
        if (target is FloydPart.Floyd)
        {
            return new PositiveInteractionResult(
                $"{BeamDescription} which strikes Floyd. " +
                "\"Yow!\" yells Floyd. He jumps to the other end of the room and eyes you warily. ");
        }

        // Special response for shooting the Relay
        if (target is Relay relay)
        {
            return LaserSpeckHelper.ShootRelay(relay, context, BeamDescription, Setting, Chooser);
        }

        var targetName = target.NounsForMatching.FirstOrDefault() ?? targetNoun;

        return new PositiveInteractionResult(
            $"{BeamDescription} which strikes the {targetName}. " +
            $"The {targetName} grows a bit warm, but nothing else happens. ");
    }

    public override Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        // Handle "shoot X with laser" - laser is NounTwo, target is NounOne
        if (action.MatchVerb(["shoot", "fire"]) &&
            action.MatchPreposition(["with"]) &&
            action.MatchNounTwo(NounsForMatching))
        {
            var targetIsLaser = action.MatchNounOne(NounsForMatching);
            var targetIsInLaser = Items.Any(item => item.NounsForMatching.Contains(action.NounOne, StringComparer.OrdinalIgnoreCase));

            if (targetIsLaser || targetIsInLaser)
                return Task.FromResult<InteractionResult?>(
                    new PositiveInteractionResult("Sorry, the laser doesn't have a rubber barrel. "));

            // Shooting at something else
            return Task.FromResult<InteractionResult?>(ShootLaserAt(action.NounOne, context));
        }

        // Handle "shoot laser at X" - laser is NounOne, target is NounTwo
        if (action.MatchVerb(["shoot", "fire"]) &&
            action.MatchPreposition(["at"]) &&
            action.MatchNounOne(NounsForMatching))
        {
            var targetIsLaser = action.MatchNounTwo(NounsForMatching);
            var targetIsInLaser = Items.Any(item => item.NounsForMatching.Contains(action.NounTwo, StringComparer.OrdinalIgnoreCase));

            if (targetIsLaser || targetIsInLaser)
                return Task.FromResult<InteractionResult?>(
                    new PositiveInteractionResult("Sorry, the laser doesn't have a rubber barrel. "));

            // Shooting at something else
            return Task.FromResult<InteractionResult?>(ShootLaserAt(action.NounTwo, context));
        }

        if (!action.MatchVerb(["turn", "set"]))
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.MatchPreposition(["to"]))
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.MatchNounOne(["dial", "laser", "setting"]))
            return base.RespondToMultiNounInteraction(action, context);

        return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(AttemptSetDial(action.NounTwo)));
    }
    
    private string AttemptSetDial(string actionNounTwo)
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

        // Use ToInteger() to support both numeric ("5") and word ("five") input
        var result = input.ToInteger();

        if (result.HasValue)
        {
            return result.Value switch
            {
                >= 1 and <= 6 => (TurnDialResult.IsNumberBetween1And6, result.Value),
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

    public override string? OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        Repository.GetItem<Floyd>().CommentOnAction(FloydPrompts.LaserPickedUp, context);
        return base.OnBeingTaken(context, previousLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return !Items.Any() ? "A laser" : $"A laser\n{ItemListDescription("laser", null)}";
    }

    /// <summary>
    /// Messages displayed when the laser warms up (threshold reached going UP).
    /// </summary>
    private static readonly Dictionary<int, string> WarmingMessages = new()
    {
        { 3, "The laser feels slightly warm now, but that doesn't seem to affect its performance at all." },
        { 6, "The laser feels somewhat warm now, but that doesn't seem to affect its performance at all." },
        { 9, "The laser feels very warm now, but that doesn't seem to affect its performance at all." },
        { 12, "The laser feels quite hot, but that doesn't seem to affect its performance at all." }
    };

    /// <summary>
    /// Messages displayed when the laser cools down (threshold reached going DOWN).
    /// </summary>
    private static readonly Dictionary<int, string> CoolingMessages = new()
    {
        { 12, "The laser has cooled, but it still feels quite hot." },
        { 9, "The laser has cooled, but it still feels very warm." },
        { 6, "The laser has cooled, but it still feels somewhat warm." },
        { 3, "The laser has cooled, but it still feels slightly warm." }
    };

    /// <summary>
    /// Determines if the player is in the same location as the laser.
    /// The laser could be in the player's inventory (CurrentLocation == context) or
    /// in the same room as the player.
    /// </summary>
    private bool IsPlayerWithLaser(IContext context)
    {
        // Laser is in player's inventory
        if (CurrentLocation == context)
            return true;

        // Laser is in the same room as the player
        if (CurrentLocation == context.CurrentLocation)
            return true;

        return false;
    }

    /// <summary>
    /// Called each turn when the laser is registered as an actor.
    /// Handles warming up when fired, cooling down when not fired.
    /// </summary>
    public Task<string> Act(IContext context, IGenerationClient client)
    {
        string message = string.Empty;

        if (JustShot)
        {
            // Laser was fired this turn - heat up
            WarmthLevel++;

            // Check for warming message at this threshold
            if (WarmingMessages.TryGetValue(WarmthLevel, out var warmingMessage))
            {
                if (IsPlayerWithLaser(context))
                {
                    message = warmingMessage;
                }
            }
        }
        else
        {
            // Laser was NOT fired this turn - cool down
            if (WarmthLevel > 0)
            {
                WarmthLevel--;

                // Check for cooling message at this threshold
                if (CoolingMessages.TryGetValue(WarmthLevel, out var coolingMessage))
                {
                    if (IsPlayerWithLaser(context))
                    {
                        message = coolingMessage;
                    }
                }

                // If fully cooled, remove from actors
                if (WarmthLevel == 0)
                {
                    context.RemoveActor(this);
                }
            }
        }

        // Reset the JustShot flag for the next turn
        JustShot = false;

        return Task.FromResult(message);
    }
}


/*
 
 Shooting the Laser at the Microbe

   The microbe is a hungry monster that blocks your path on the silicon strip (inside the computer). Here's how the laser interaction works:

   Direct Laser Hits

   - Setting 1 (red): The beam "passes harmlessly through its red skin" — the microbe's skin is red, so red light doesn't affect it
   - Settings 2-6 (orange through violet): The beam hits and the microbe recoils momentarily, but quickly recovers. You get random flavor text like:
     - "The microbe's outer membrane sizzles a bit, and some protoplasm oozes out"
     - "The beam slices through the microbe's skin! A tremendous shudder passes..."
     - "The monster rears back for a moment, but almost as soon as the beam goes off, it advances again"

   You cannot kill the microbe by shooting it directly — it always regenerates.

   The Heat Mechanic (How to Actually Defeat It)

   Each time you fire the laser, WARMTH-FLAG increases. The laser heats up progressively: "slightly warm" → "somewhat warm" → "very warm" → "quite hot"

   Once the laser is hot enough (WARMTH-FLAG > 7), you have options:

   1. Throw the hot laser off the strip: The microbe, attracted to the heat, lunges after it and both plummet into the void (comptwo.zil:3019-3029)
   2. Give/throw the hot laser to the microbe:
     - If WARMTH-FLAG > 10: The microbe eats the laser, writhes in pain from the heat, and rolls off the strip
     - If WARMTH-FLAG ≤ 10: The microbe eats the laser and turns toward you (bad outcome)
   3. Danger: If WARMTH-FLAG > 13 and you're still holding the laser when the microbe attacks, it lunges at the pulsing heat, you lose your balance, and both of you fall into the void (death)

   The solution is to heat up the laser by firing it several times, then sacrifice it to lure the microbe to its doom.
 
 */
