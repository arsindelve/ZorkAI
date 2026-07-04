using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.StaticCommand.Implementation;

/// <summary>
/// I can't make you not use this, or not be aware of it's existence - open source after all.
/// But seriously, I use this for testing and debugging. If you use it while playing the game,
/// you'll ruin your experience. Resist the urge. 
/// </summary>
public class GodModeProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentNullException(nameof(input));

        if (input.Contains(" take ")) return Task.FromResult(Take(input, context));
        if (input.Contains(" go ")) return Task.FromResult(Go(input, context));
        if (input.Contains(" where ")) return Task.FromResult(Where(input));
        if (input.Contains(" kill ")) return Task.FromResult(Kill(input, context));

        if (context is IResettableClockContext clockContext &&
            ResetClock(input, clockContext) is { } clockResetResult)
            return Task.FromResult(clockResetResult);

        // Issue #277: toggle the survival clocks (sleep/hunger) for deterministic playtesting. Only
        // games whose context tracks those clocks (Planetfall) implement ISurvivalClockContext.
        if (context is ISurvivalClockContext survivalContext &&
            ToggleSurvivalClocks(input, survivalContext) is { } survivalResult)
            return Task.FromResult(survivalResult);

        // Game-specific god-mode subcommands (e.g. Planetfall's companion-wandering toggle) live on
        // the game's own context via IGodModeCommandHandler, so game concepts never leak into the
        // engine. Checked last: a game can add commands but not shadow the built-ins above.
        if (context is IGodModeCommandHandler gameHandler &&
            gameHandler.HandleGodModeCommand(input) is { } gameResult)
            return Task.FromResult(gameResult);

        return Task.FromResult("Invalid use of God mode. Bad adventurer! ");
    }

    private static string? ResetClock(string input, IResettableClockContext context)
    {
        var words = input.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (!words.Contains("reset") || (!words.Contains("time") && !words.Contains("clock")))
            return null;

        const int walkthroughResetTime = 2000;
        context.ResetClockForGodMode(walkthroughResetTime);
        return $"God mode: chronometer reset to {walkthroughResetTime}.";
    }

    /// <summary>
    /// Issue #277: recognizes "god mode [no] sleep|hunger|survival" (with optional "on"/"off") and
    /// flips the corresponding survival-clock flag on the context. "no"/"off" disables a clock;
    /// anything else re-enables it. "survival" affects both clocks at once. Returns the confirmation
    /// message, or null if the input names no recognized clock (so the caller falls through to the
    /// generic god-mode error).
    /// </summary>
    private static string? ToggleSurvivalClocks(string input, ISurvivalClockContext context)
    {
        var words = input.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var affectsSleep = words.Contains("sleep") || words.Contains("survival");
        var affectsHunger = words.Contains("hunger") || words.Contains("survival");

        // Not a survival-clock command - let the caller emit the generic error.
        if (!affectsSleep && !affectsHunger)
            return null;

        // "no" / "off" disables the clock; a bare verb (or explicit "on") re-enables it.
        var disable = words.Contains("no") || words.Contains("off");

        var affected = new List<string>();
        var effects = new List<string>();
        if (affectsSleep)
        {
            context.SleepClockDisabled = disable;
            affected.Add("sleep");
            effects.Add("tired");
        }

        if (affectsHunger)
        {
            context.HungerClockDisabled = disable;
            affected.Add("hunger");
            effects.Add("hungry");
        }

        var clocks = string.Join(" and ", affected);
        var noun = affected.Count > 1 ? "clocks" : "clock";
        return disable
            ? $"God mode: {clocks} {noun} disabled. You will no longer get {string.Join(" or ", effects)}. "
            : $"God mode: {clocks} {noun} enabled. ";
    }

    private string Where(string input)
    {
        var lastWord = GetWordsAfterTarget(input, "where");
        return Repository.GetItem(lastWord)?.CurrentLocation?.Name ?? "Unknown";
    }

    private string Go(string input, IContext context)
    {
        Repository.LoadAllLocations(context.Game.GameName);
        var lastWord = GetWordsAfterTarget(input, "go");
        var location = Repository.GetLocation(lastWord);

        if (location == null)
            return "Invalid use of God mode. Bad adventurer! ";

        // Issue #241: do NOT Init() here. Repository.LoadAllLocations (above) already creates and
        // Init()s any missing location exactly once - and a previously-loaded one was Init()'d when
        // it was first created. Re-initializing would run Init() twice, and locations that use
        // StartWithItem<T> (a bare Items.Add with no dedupe) would end up with their starting items
        // duplicated in the room.
        context.CurrentLocation = location;

        // Issue #356 follow-up: this is a raw location swap, not a real move - it skips
        // OnLeaveLocation/AfterEnterLocation, so a game-specific location-blind death clock (e.g.
        // Planetfall's Feinstein explosion) never gets the chance to disarm itself. Let the context
        // do that explicitly for a god-mode teleport.
        if (context is IGodModeTeleportAware teleportAware)
            teleportAware.OnGodModeTeleport();

        return $"Welcome to {location.Name}";
    }

    private string Take(string input, IContext context)
    {
        Repository.LoadAllItems(context.Game.GameName);
        var lastWord = GetWordsAfterTarget(input, "take");
        var item = Repository.GetItem(lastWord);
        if (item == null)
            return "Invalid use of God mode. Bad adventurer! ";

        context.ItemPlacedHere(item);
        return $"I hope you enjoy your {item.Name}";
    }

    /// <summary>
    /// Issue #374: deterministically kills a combat-gate creature (e.g. the Zork I troll, thief, or
    /// cyclops) so playtesting can proceed past it without relying on randomized combat. Only
    /// creatures that implement <see cref="ICanBeAttacked"/> and opt into
    /// <see cref="ICanBeAttacked.GodModeKill"/> support this; everything else falls through to the
    /// generic god-mode error.
    /// </summary>
    private string Kill(string input, IContext context)
    {
        Repository.LoadAllItems(context.Game.GameName);
        var lastWord = GetWordsAfterTarget(input, "kill");
        var item = Repository.GetItem(lastWord);

        if (item is not ICanBeAttacked attackable || !attackable.GodModeKill(context))
            return "Invalid use of God mode. Bad adventurer! ";

        return $"God mode: {item.Name} is dead. ";
    }

    private static string GetWordsAfterTarget(string input, string targetWord)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(targetWord))
            return string.Empty;

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var indexOfTarget = Array.IndexOf(words, targetWord);

        if (indexOfTarget == -1 || indexOfTarget == words.Length - 1)
            return string.Empty;

        return string.Join(" ", words.Skip(indexOfTarget + 1));
    }
}
