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
///
/// Matching is on whole WORDS of the room's name (plus any opt-in synonyms), so the player can use a
/// natural short form — "enter the shuttle" -> "Shuttle Car Betty", "go to the office" -> "Brig
/// Office" — rather than the exact room title. A name that resolves to two+ neighbours asks "which
/// one?".
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

            if (NameMatches(room, target))
                matches.Add((dir, room, movement!.CanGo(context)));
        }

        var deduped = matches
            .GroupBy(m => m.Room)
            .Select(g => g.OrderByDescending(m => m.CanGo).First())
            .ToList();

        // Prefer rooms we can actually reach right now. Only offer a gated match (a closed door, or an
        // absent shuttle car whose exit is still on the map) when NOTHING currently matching is
        // passable — that lone gated match then lets MoveEngine surface its specific failure ("The
        // kitchen window is closed.") instead of pretending the room isn't there, while a present
        // neighbour is never drowned out by an absent one.
        var passable = deduped.Where(m => m.CanGo).ToList();
        var chosen = passable.Count > 0 ? passable : deduped;

        // Drop indistinguishable repeats: when 2+ matched rooms share the same name (the maze's 15
        // identical "Maze" rooms, the four "Forest" rooms, the two "Cave"s), the player cannot pick one
        // by name and a "the Maze or the Maze?" prompt would be nonsense — these wander-areas must not
        // be navigable by name. A uniquely-named match (e.g. entering the maze from a named entrance)
        // still resolves. This is what keeps destination navigation from making mazes WORSE.
        // Group by the normalized name once (rather than normalizing each name twice — once to count,
        // once to filter); keep only the groups holding a single room.
        return chosen
            .GroupBy(m => Normalize(m.Room.Name))
            .Where(g => g.Count() == 1)
            .SelectMany(g => g)
            .Select(m => (m.Direction, m.Room))
            .ToList();
    }

    /// <summary>
    /// Builds the engine's existing "which one?" disambiguation when a name matches more than one
    /// adjacent room (e.g. "enter the shuttle" when both cars are at the platform, or "enter elevator"
    /// in the Elevator Lobby). Reply keys are each room's DISTINGUISHING words ("betty", "upper") plus
    /// any non-shared synonyms; the replacement re-issues "go to the &lt;full name&gt;" so the follow-up
    /// resolves to exactly one room.
    /// </summary>
    public static DisambiguationInteractionResult BuildDisambiguation(
        IReadOnlyList<(Direction Direction, ILocation Room)> matches)
    {
        var wordsByRoom = matches.Select(m => (m.Room, Words: WordsOf(m.Room))).ToList();

        // A word shared by 2+ matched rooms ("shuttle", "car", "elevator") can't tell them apart, so it
        // must never be a reply key. Counting DISTINCT rooms (WordsOf is already per-room distinct)
        // means a word a single room merely repeats isn't mistaken for shared.
        var shared = wordsByRoom
            .SelectMany(r => r.Words)
            .GroupBy(w => w)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        var responses = new Dictionary<string, string>();
        foreach (var (room, words) in wordsByRoom)
        {
            // Re-issue the FULL, unambiguous room name; "go to the <name>" then resolves to exactly
            // this room.
            var canonical = Normalize(room.Name);

            // Reply keys: the room's distinguishing words, plus any of its synonyms that aren't
            // themselves a shared word, plus its full name. We drop single-character keys ("c"/"d" for
            // "Dorm C"/"Dorm D"): the DisambiguationProcessor matches replies by SUBSTRING, so a lone
            // letter would collide with any reply that incidentally contains it ("the se-c-ond one").
            // The full name always remains a usable, unambiguous answer.
            var keys = words.Where(w => !shared.Contains(w))
                .Concat(Terms(room).Select(Normalize).Where(t => !shared.Contains(t)))
                .Where(k => k.Length > 1)
                .Append(canonical);

            foreach (var key in keys)
                responses[key] = canonical;
        }

        var message = "Which do you mean, " +
                      string.Join(" or ", matches.Select(m => $"the {m.Room.Name}")) + "?";

        return new DisambiguationInteractionResult(message, responses, "go to the {0}");
    }

    // Everything a player may call this room: its display name plus any opt-in synonyms. Overrides of
    // ILocation.NounsForMatching ADD aliases (a colour, "kitchen" for a room titled "Mess Hall"); they
    // never have to repeat the name, since the name's own words already match.
    private static IEnumerable<string> Terms(ILocation room) => room.NounsForMatching.Prepend(room.Name);

    // True when `target` appears as a whole-word phrase within one of the room's terms, so "shuttle"
    // matches "Shuttle Car Betty" and "office" matches "Brig Office". Exact equality is the degenerate
    // case (the padded form makes " office " contain " office ").
    private static bool NameMatches(ILocation room, string target) =>
        Terms(room).Any(t => $" {Normalize(t)} ".Contains($" {target} "));

    private static HashSet<string> WordsOf(ILocation room) =>
        Terms(room)
            .SelectMany(t => Normalize(t).Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToHashSet();

    private static readonly string[] LeadingArticles = ["the ", "a ", "an "];

    private static string Normalize(string s)
    {
        s = s.Trim().ToLowerInvariant();
        foreach (var article in LeadingArticles)
            if (s.StartsWith(article))
                return s[article.Length..];
        return s;
    }
}
