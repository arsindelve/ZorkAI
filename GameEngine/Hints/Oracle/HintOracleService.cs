using GameEngine.Hints.Data;
using Model.Hints;
using Model.Interface;

namespace GameEngine.Hints.Oracle;

/// <summary>What a game supplies to the oracle: its voice, its complete knowledge, and how to describe a player's situation.</summary>
public interface IOracleProvider
{
    HintPersona Persona { get; }

    /// <summary>The game bible: a veteran player's understanding of the whole game, in prose. Static.</summary>
    string GameKnowledge { get; }

    /// <summary>The facts of this player's game right now (not their transcript — the service adds that).</summary>
    string DescribeSituation(IContext state);
}

/// <param name="Transcript">The recent stretch of the player's game as they saw it (commands and responses), oldest first.</param>
public sealed record OracleHintRequest(
    IContext State, string Question, IReadOnlyList<HintExchange> History, string? Transcript = null);

/// <summary>
///     The whole hint engine: describe the player's situation, hand it to the oracle with the game's
///     knowledge and the conversation, return what it says. Stateless and read-only; fails closed (an
///     unavailable oracle is a decline, never a guess).
/// </summary>
public sealed class HintOracleService(IOracleProvider provider, IHintOracle oracle)
{
    public const string Unavailable = "The narrator seems to have stepped away for a moment. Try again shortly.";
    private const int TranscriptChars = 9000;
    private const int HistoryTurns = 20;

    public async Task<(string Text, bool IsHint)> GetHint(OracleHintRequest request)
    {
        var situation = provider.DescribeSituation(request.State) + DescribeTranscript(request.Transcript);
        var answer = await oracle.Answer(provider.GameKnowledge, provider.Persona, situation,
            request.History.TakeLast(HistoryTurns).ToList(), request.Question?.Trim() ?? string.Empty);

        return answer is null ? (Unavailable, false) : (answer, true);
    }

    private static string DescribeTranscript(string? transcript)
    {
        var text = transcript?.Trim();
        if (string.IsNullOrEmpty(text)) return string.Empty;

        if (text.Length > TranscriptChars) text = "… " + text[^TranscriptChars..];
        return "\n\nTHE LAST STRETCH OF THEIR GAME, AS THEY SAW IT (oldest first):\n" + text;
    }
}

/// <summary>
///     What has changed in the world since the game began — read mechanically from the live state against a
///     baseline captured at game start, so nothing about any particular puzzle is encoded here. It is how the
///     oracle knows what the player has already done (the ladder is across the rift, the padlock is off,
///     Floyd has been switched on) beyond what their inventory and recent transcript show.
/// </summary>
public static class WorldDelta
{
    // Clocks, counters and engine plumbing: they change every turn and say nothing about progress.
    private static readonly System.Text.RegularExpressions.Regex Noise = new(
        "(Moves|Time|Countdown|Counter|Remaining|Timer|LastRolled|Speed|ThisTurn|Chooser|Generation|Turns|Callback" +
        "|Description|LastLocation|PreviousLocation|TurnFlags|VisitCount|Notifications)",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

    public static IReadOnlyList<string> Describe(IReadOnlyDictionary<string, string> baseline, IReadOnlyDictionary<string, string> now,
        int limit = 160)
    {
        var lines = new List<string>();
        foreach (var (key, value) in now.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var property = key[(key.LastIndexOf('.') + 1)..];
            if (Noise.IsMatch(property)) continue;

            if (baseline.TryGetValue(key, out var was))
            {
                if (was == value) continue;
            }
            else if (value is "False" or "0" or StateFlattener.Null or "list:" or "")
            {
                continue; // an object the engine registered late, still at its defaults
            }

            lines.Add($"{Readable(key)} = {Readable(value)}" + (was is null ? "" : $" (was {Readable(was)})"));
            if (lines.Count >= limit) break;
        }

        return lines;
    }

    public static IReadOnlyList<string> VisitedRooms(IReadOnlyDictionary<string, string> now) =>
        now.Where(kv => kv.Key.StartsWith("Location:", StringComparison.Ordinal) &&
                        kv.Key.EndsWith(".VisitCount", StringComparison.Ordinal) && kv.Value != "0")
            .Select(kv => Words(kv.Key["Location:".Length..^".VisitCount".Length]))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

    private static string Readable(string text) =>
        text.StartsWith("list:", StringComparison.Ordinal) ? "[" + text[5..].Replace("|", ", ") + "]"
        : text.Replace("Item:", "").Replace("Location:", "");

    /// <summary>"AdminCorridorSouth" -> "Admin Corridor South".</summary>
    private static string Words(string typeName) =>
        System.Text.RegularExpressions.Regex.Replace(typeName, "(?<=[a-z0-9])(?=[A-Z])", " ");
}
