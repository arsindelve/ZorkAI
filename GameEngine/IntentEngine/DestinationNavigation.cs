using Model.Interaction;
using Model.Interface;
using Model.Location;
using Model.Movement;

namespace GameEngine.IntentEngine;

/// <summary>
/// Destination-based navigation (issue #268): turns a typed ROOM NAME into a single move, but only
/// when that room is a direct exit of the room the player is standing in. We deliberately do NOT
/// pathfind, walk multiple rooms, or index rooms the player hasn't seen — you can only name a
/// neighbour, which inherently can't leak the map and lets first-time entry (e.g. "enter the kitchen"
/// from Behind House) work.
/// </summary>
internal static class DestinationNavigation
{
    // Every direction a Map can define an exit for. We probe each because the only public way to ask
    // "where does this exit lead?" is ILocation.Navigate(direction) — the Map itself is protected.
    private static readonly Direction[] AllDirections =
    [
        Direction.N, Direction.S, Direction.E, Direction.W,
        Direction.NE, Direction.NW, Direction.SW, Direction.SE,
        Direction.In, Direction.Out, Direction.Up, Direction.Down
    ];

    /// <summary>
    /// Every exit of the CURRENT room whose destination room matches the typed name, deduped by room
    /// (a room reachable two ways counts as one; we keep a currently-passable direction when there is
    /// one so an open route is preferred over a gated one). Empty when nothing matches.
    /// </summary>
    public static IReadOnlyList<(Direction Direction, ILocation Room)> ResolveAllAdjacent(
        string? destination, IContext context)
    {
        var matches = new List<(Direction Direction, ILocation Room)>();
        if (string.IsNullOrWhiteSpace(destination))
            return matches;

        var target = Normalize(destination);

        foreach (var dir in AllDirections)
        {
            // Navigate() returns the MovementParameters for this exit even when CanGo is currently
            // false (e.g. a closed door), so we can match the room name now and still let MoveEngine
            // surface the gated failure later. Location is null for blocked/message-only exits.
            var room = context.CurrentLocation.Navigate(dir, context)?.Location;
            if (room is null)
                continue;

            if (room.NounsForMatching.Any(n => Normalize(n).Equals(target)))
                matches.Add((dir, room));
        }

        return matches
            .GroupBy(m => m.Room)
            .Select(g => g
                .OrderByDescending(m => context.CurrentLocation.Navigate(m.Direction, context)!.CanGo(context))
                .First())
            .ToList();
    }

    /// <summary>
    /// Builds the engine's existing "which one?" disambiguation when a name matches more than one
    /// adjacent room (e.g. "enter elevator" in the Elevator Lobby). Reply keys are each room's
    /// DISTINGUISHING synonyms (those not shared across the matched rooms) plus its name; the
    /// replacement re-issues "go to the &lt;canonical&gt;" so the follow-up resolves to exactly one room.
    /// </summary>
    public static DisambiguationInteractionResult BuildDisambiguation(
        IReadOnlyList<(Direction Direction, ILocation Room)> matches)
    {
        // Synonyms shared by more than one matched room are the ambiguous ones (e.g. "elevator") and
        // must never be reply keys, or the answer would be ambiguous all over again.
        var shared = matches
            .SelectMany(m => m.Room.NounsForMatching.Select(Normalize))
            .GroupBy(s => s)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        var responses = new Dictionary<string, string>();
        foreach (var (_, room) in matches)
        {
            var distinguishing = room.NounsForMatching.Where(n => !shared.Contains(Normalize(n))).ToList();
            // The canonical re-resolves to THIS room only (it is a non-shared synonym), so the
            // re-issued "go to the {canonical}" lands on a single match.
            var canonical = distinguishing.FirstOrDefault() ?? room.Name;
            foreach (var key in distinguishing.Append(room.Name))
                responses[Normalize(key)] = canonical;
        }

        var message = "Which do you mean, " +
                      string.Join(" or ", matches.Select(m => $"the {m.Room.Name}")) + "?";

        return new DisambiguationInteractionResult(message, responses, "go to the {0}");
    }

    private static string Normalize(string s)
    {
        s = s.Trim().ToLowerInvariant();
        foreach (var article in new[] { "the ", "a ", "an " })
            if (s.StartsWith(article))
                return s[article.Length..];
        return s;
    }
}
