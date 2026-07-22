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

        // Game-specific god-mode subcommands (e.g. Planetfall's chronometer reset, survival-clock and
        // companion-wandering toggles) live on the game's own context via IGodModeCommandHandler, so
        // game concepts never leak into the engine. Checked last: a game can add commands but not
        // shadow the built-ins above.
        if (context is IGodModeCommandHandler gameHandler &&
            gameHandler.HandleGodModeCommand(input) is { } gameResult)
            return Task.FromResult(gameResult);

        return Task.FromResult("Invalid use of God mode. Bad adventurer! ");
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
        var teleportNote = context is IGodModeTeleportAware teleportAware
            ? teleportAware.OnGodModeTeleport()
            : null;

        return $"Welcome to {location.Name}" + (teleportNote is null ? "" : $". {teleportNote}");
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
