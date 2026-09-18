using System.Text;
using System.Text.Json;
using OpenAI.Chat;
using ZorkAI.OpenAI;

namespace Planetfall.Tests.Hints.Generator;

public sealed record WrittenLadder(string Title, string[] Rungs);

/// <summary>
///     Writes a puzzle's three rungs from its verified commands and the engine's actual responses.
///     "Fed, not generated": the model rephrases what the walkthrough did; it is told what it may not
///     mention (the words of later puzzles) and that rung C must be the commands themselves.
/// </summary>
public sealed class RungWriter : OpenAIClientBase
{
    protected override string ModelName => "gpt-5.4-mini";

    private RungWriter() : base(null, requireApiKey: true)
    {
    }

    /// <param name="exit">The short walk to the next puzzle, to close rung C with ("then go port"); null for none.</param>
    public static Task<WrittenLadder?> Write(Segment segment, IReadOnlyList<string> doNotMention, bool navigation, string? exit) =>
        new RungWriter().WriteLadder(segment, doNotMention, navigation, exit);

    private async Task<WrittenLadder?> WriteLadder(Segment segment, IReadOnlyList<string> doNotMention, bool navigation, string? exit)
    {
        const string common =
            "You write progressive hints for the Infocom text adventure Planetfall. Reply with ONLY a JSON object " +
            "{\"title\": \"...\", \"rungs\": [\"A\", \"B\", \"C\"]}.\n" +
            "title: three to eight words naming what the player accomplishes (an imperative: 'Wake the robot').\n" +
            "Rules: use only what is in the transcript. Never mention anything in the DO NOT MENTION list. Never hint " +
            "at later puzzles. Plain prose, no markdown, each rung one or two sentences.";

        var system = navigation
            ? common + "\nThis is a NAVIGATION puzzle: the transcript is the route the player took from one place to " +
              "the next place something happens.\nA: the vaguest nudge — which general direction or kind of place to " +
              "head for, and any event to wait for on the way. No directions.\nB: the route in prose (landmarks, " +
              "turns), without listing the exact commands.\nC: the exact commands, in order, comma-separated, " +
              "prefixed once by the starting room; write repeated waits as 'wait ×N'."
            : common + "\nThe transcript is ONE puzzle: the exact commands a player typed in one room, in order, with " +
              "the game's responses.\nA: the vaguest orienting nudge — what to notice or think about. No commands. " +
              "Name nothing the player has not already seen in this room.\nB: the approach — the object, place or " +
              "idea and what to do with it, in prose, without the exact commands.\nC: the exact commands, in order, " +
              "comma-separated, prefixed once by the room name; leave out 'look' and write repeated waits as 'wait ×N'.";

        var sb = new StringBuilder();
        if (navigation)
        {
            sb.AppendLine($"FROM: {segment.ApproachFrom}    TO: {segment.Location}");
            sb.AppendLine("THE ROUTE, AND WHAT THE GAME SAID:");
            foreach (var (command, response) in segment.ApproachExchanges)
                sb.AppendLine($"> {command}\n  {Trim(response, 200)}");
        }
        else
        {
            sb.AppendLine($"ROOM: {segment.Location}");
            if (segment.Approach.Count > 0)
                sb.AppendLine($"HOW THE PLAYER GOT HERE (from the {segment.ApproachFrom}): {string.Join(", ", segment.Approach.TakeLast(6))}");
            sb.AppendLine("WHAT THEY TYPED, AND WHAT THE GAME SAID:");
            foreach (var (command, response) in segment.Exchanges)
                sb.AppendLine($"> {command}\n  {Trim(response, 240)}");
            if (exit is not null)
                sb.AppendLine($"THEN THE PLAYER LEFT THE ROOM BY: {exit}   (end rung C with this: ', then {exit}')");
        }

        if (doNotMention.Count > 0)
            sb.AppendLine($"DO NOT MENTION: {string.Join(", ", doNotMention)}");

        var messages = new List<ChatMessage> { new SystemChatMessage(system), new UserChatMessage(sb.ToString()) };
        string raw;
        try
        {
            raw = await Client!.CompleteChatAsync(messages, new ChatCompletionOptions { Temperature = 0.2f });
        }
        catch (Exception)
        {
            return null;
        }

        var json = LlmJson.ExtractJsonObject(raw);
        if (json is null) return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var title = doc.RootElement.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
            var rungs = doc.RootElement.TryGetProperty("rungs", out var r) && r.ValueKind == JsonValueKind.Array
                ? r.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToArray()
                : Array.Empty<string>();
            return rungs.Length == 3 && title.Length > 0 ? new WrittenLadder(title, rungs) : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Trim(string text, int max)
    {
        var flat = text.Replace("\r", " ").Replace("\n", " ").Trim();
        while (flat.Contains("  ")) flat = flat.Replace("  ", " ");
        return flat.Length <= max ? flat : flat[..max] + "…";
    }
}
