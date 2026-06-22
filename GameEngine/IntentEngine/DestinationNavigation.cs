using Model.AIGeneration;
using Model.Intent;
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
    // Every real direction a Map can define an exit for, derived from the enum so a newly added
    // Direction is probed automatically. We probe each because the only public way to ask "where does
    // this exit lead?" is ILocation.Navigate(direction) — the Map itself is protected.
    private static readonly Direction[] AllDirections =
        Enum.GetValues<Direction>().Where(d => d != Direction.Unknown).ToArray();

    /// <summary>
    /// Resolve a typed room name against the current room's exits and produce the turn result:
    /// a single move (one match), a "which one?" disambiguation (two+ matches), or <c>null</c> when no
    /// adjacent room matches — leaving the caller to decide the context-appropriate refusal. Shared by
    /// both the "go to &lt;room&gt;" and "enter &lt;room&gt;" entry points so the move/disambiguate flow lives in
    /// one place.
    /// </summary>
    public static async Task<(InteractionResult? resultObject, string ResultMessage)?> TryNavigate(
        string? destination, IContext context, IGenerationClient generationClient)
    {
        var matches = ResolveAllAdjacent(destination, context);

        switch (matches.Count)
        {
            case 0:
                return null;

            case 1:
                // Single hop: reuse the full movement pipeline so a gated exit yields its own message
                // ("The kitchen window is closed.") instead of a teleport, and the leave/enter hooks
                // and turn side-effects all fire normally.
                return await new MoveEngine().Process(
                    new MoveIntent { Direction = matches[0].Direction }, context, generationClient);

            default:
                // Ambiguous → hand the engine's existing disambiguation flow a prompt. The
                // InteractionMessage must ALSO be the ResultMessage so the player sees the question this
                // turn (mirrors SimpleInteractionEngine's disambiguation return).
                var disambiguation = BuildDisambiguation(matches);
                return (disambiguation, disambiguation.InteractionMessage);
        }
    }

    /// <summary>
    /// Every exit of the CURRENT room whose destination room matches the typed name, deduped by room
    /// (a room reachable two ways counts as one; we keep a currently-passable direction when there is
    /// one so an open route is preferred over a gated one). Empty when nothing matches.
    /// </summary>
    public static IReadOnlyList<(Direction Direction, ILocation Room)> ResolveAllAdjacent(
        string? destination, IContext context)
    {
        if (string.IsNullOrWhiteSpace(destination))
            return [];

        var target = Normalize(destination);

        // Probe each exit exactly once and capture its passability now, so we never re-Navigate (and
        // re-build the room's Map) while deduping/ordering below.
        var matches = new List<(Direction Direction, ILocation Room, bool CanGo)>();
        foreach (var dir in AllDirections)
        {
            // Navigate() returns the MovementParameters for this exit even when CanGo is currently
            // false (e.g. a closed door), so we can match the room name now and still let MoveEngine
            // surface the gated failure later. Location is null for blocked/message-only exits.
            var movement = context.CurrentLocation.Navigate(dir, context);
            var room = movement?.Location;
            if (room is null)
                continue;

            if (room.NounsForMatching.Any(n => Normalize(n).Equals(target)))
                matches.Add((dir, room, movement!.CanGo(context)));
        }

        return matches
            .GroupBy(m => m.Room)
            .Select(g => g.OrderByDescending(m => m.CanGo).First())
            .Select(m => (m.Direction, m.Room))
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
        // A synonym is "shared" (and so must never be a reply key) when it belongs to more than one of
        // the matched ROOMS. We Distinct() each room's nouns first so a synonym a single room happens
        // to list twice (e.g. "dome" and "the dome" both normalizing to "dome") is not mistaken for
        // shared and stripped from that room's distinguishing keys.
        var shared = matches
            .SelectMany(m => m.Room.NounsForMatching.Select(Normalize).Distinct())
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
