using System.Text;
using Newtonsoft.Json.Linq;
using ZorkAI.OpenAI;

namespace Planetfall.Tests.Hints.Oracle;

public enum Verdict
{
    Good,
    Weak,
    Wrong,
    Spoiler
}

public sealed record Grade(Verdict Verdict, string Reason);

/// <summary>
///     Grades a hint exchange the way a person reading the transcript would — against ground truth the hint
///     system never sees: the test author's note on the moment, the verified walkthrough's next commands, and
///     the reference material (official hint book, lore notes). A different model from the oracle's, so it is
///     not marking its own homework.
/// </summary>
public static class HintGrader
{
    private static string Model => Environment.GetEnvironmentVariable("HINT_GRADER_MODEL") is { Length: > 0 } m ? m : "gpt-5.5";

    private const string System =
        "You are grading a hint system for the text adventure Planetfall. A player, at a known point in a verified " +
        "playthrough, asked the guide one or more things in a row; you see each question and the guide's reply. You " +
        "also have GROUND TRUTH: a note on the moment from the test author (the guide did not see it), the player's " +
        "actual game state (the guide saw this), the next commands of the verified walkthrough from this point, and " +
        "REFERENCE material (the official hint book and lore notes). The walkthrough is a test script: where it says " +
        "a god-mode hook is applied (resetting the clock, for instance), a real player in this state cannot simply do " +
        "that step, and a guide that says so — from the player's real clock, health or inventory — is RIGHT, not " +
        "wrong. You see only the next few commands: a guide that correctly describes what follows them is not wrong " +
        "for that. Trust the state over the author's note about what has or has not been done yet, and do not " +
        "penalise a brief, relevant warning about a way to make the game unwinnable.\n\n" +
        "A good guide: answers the question that was asked (a story question gets story, not a puzzle step); is " +
        "right about the game; points at what actually stands between THIS player and progress; starts with a nudge " +
        "and gets more explicit only as the player asks again ('more', 'just tell me'), ending in exact commands when " +
        "pressed; says plainly when something is a dead end; and never reveals things the player has not yet " +
        "encountered — later rooms, later puzzles, story revelations that come later — beyond the single step ahead " +
        "they need.\n\n" +
        "Give ONE verdict for the whole exchange:\n" +
        "GOOD — a player would be well served: on the question, correct, appropriately graded, no spoilers.\n" +
        "WEAK — not wrong, but not much help: vague past the point of usefulness, dodges the question, fails to get " +
        "more specific when asked for more, declines a fair question, or is needlessly explicit on the very first ask.\n" +
        "WRONG — misleads: points at the wrong puzzle, place or action for this moment; contradicts the walkthrough " +
        "or the reference; treats a dead end as the way forward; or answers a different question than was asked.\n" +
        "SPOILER — reveals something well beyond where the player is (a later puzzle's solution, a later area, a " +
        "story revelation not yet reachable).\n" +
        "When torn between two, choose the more severe. Reply with ONLY a JSON object: " +
        "{\"verdict\": \"GOOD\" | \"WEAK\" | \"WRONG\" | \"SPOILER\", \"reason\": \"one sentence\"}.";

    public static async Task<Grade> Judge(string reference, string note, string situation, IReadOnlyList<string> nextCommands,
        IReadOnlyList<(string Question, string Answer)> exchange)
    {
        var user = new StringBuilder();
        user.AppendLine("REFERENCE:");
        user.AppendLine(reference);
        user.AppendLine();
        user.AppendLine($"THE MOMENT (test author's note): {note}");
        user.AppendLine("THE PLAYER'S ACTUAL STATE:");
        user.AppendLine(situation);
        user.AppendLine($"THE VERIFIED WALKTHROUGH CONTINUES: {string.Join(", ", nextCommands)}");
        user.AppendLine();
        user.AppendLine("THE EXCHANGE:");
        foreach (var (question, answer) in exchange)
        {
            user.AppendLine($"PLAYER: {question}");
            user.AppendLine($"GUIDE: {answer}");
        }

        var raw = await OpenAiDirect.Ask(Model, System, user.ToString());
        var json = LlmJson.ExtractJsonObject(raw);
        if (json is null) return new Grade(Verdict.Weak, "(grader returned no JSON) " + raw[..Math.Min(raw.Length, 120)]);

        var parsed = JObject.Parse(json);
        var verdict = parsed["verdict"]?.ToString().ToUpperInvariant() switch
        {
            "GOOD" => Verdict.Good,
            "WRONG" => Verdict.Wrong,
            "SPOILER" => Verdict.Spoiler,
            _ => Verdict.Weak
        };
        return new Grade(verdict, parsed["reason"]?.ToString() ?? "");
    }
}
