using System.Text;
using GameEngine.Hints.Oracle;
using Model.Hints;
using Model.Interface;
using Planetfall.Hints;
using Planetfall.Tests.Walkthrough;
using ZorkAI.OpenAI;
using Checkpoint = Planetfall.Tests.Hints.Oracle.HintCheckpoints.Checkpoint;

namespace Planetfall.Tests.Hints.Oracle;

/// <summary>
///     The acceptance test for the hint system: sixty moments along the verified walkthrough, a stuck player's
///     questions at each, and a grader with ground truth the hint system never sees (HintGrader) reading every
///     exchange: GOOD / WEAK / WRONG / SPOILER.
///     [Explicit]: live OpenAI (OPEN_AI_KEY). HINT_EVAL_OUT=path writes the graded transcript.
/// </summary>
[TestFixture]
[Explicit("Live OpenAI graded hint evaluation — requires OPEN_AI_KEY")]
public class OraclePlaythroughEval : WalkthroughTestBase
{
    private const int NextCommandsShown = 8;

    private sealed record Moment(string Set, Checkpoint Checkpoint, string Location, string Situation, IReadOnlyList<string> Next,
        Task<List<(string Question, string Answer, string Label)>> Conversation);

    [Test]
    public Task Grade_Oracle() => Run("oracle", OracleConversation);

    // ---- the two systems under test ---------------------------------------------------------------

    /// <summary>The oracle sees a frozen description of the moment, so conversations can run while the replay moves on.</summary>
    private Task<List<(string, string, string)>> OracleConversation(Checkpoint checkpoint, string transcript)
    {
        var live = new PlanetfallOracleProvider();
        var frozen = new FrozenProvider(live.Persona, live.GameKnowledge, live.DescribeSituation(Context));
        var service = new HintOracleService(frozen, new OpenAiHintOracle());
        var context = Context;

        return Throttled(async () =>
        {
            var history = new List<HintExchange>();
            var log = new List<(string, string, string)>();
            foreach (var question in checkpoint.Questions)
            {
                var (text, isHint) = await service.GetHint(new OracleHintRequest(context, question, history, transcript));
                if (isHint) history.Add(new HintExchange(question, text));
                log.Add((question, text, isHint ? "oracle" : "unavailable"));
            }

            return log;
        });
    }

    private sealed class FrozenProvider(HintPersona persona, string knowledge, string situation) : IOracleProvider
    {
        public HintPersona Persona => persona;
        public string GameKnowledge => knowledge;
        public string DescribeSituation(IContext state) => situation;
    }

    // ---- the run ------------------------------------------------------------------------------------

    private async Task Run(string system, Func<Checkpoint, string, Task<List<(string Question, string Answer, string Label)>>> converse)
    {
        var root = WalkthroughSource.RepoRoot();
        var reference =
            "THE OFFICIAL HINT BOOK:\n" + await File.ReadAllTextAsync(Path.Combine(root, "Docs", "hints", "planetfall", "invisiclues.md")) +
            "\n\nLORE NOTES:\n" + await File.ReadAllTextAsync(Path.Combine(root, "Docs", "hints", "planetfall", "05-lore.md"));

        var steps = WalkthroughSource.Load("WalkthroughTestOne.cs");
        var all = new[]
            {
                ("puzzles", HintCheckpoints.Checkpoints),
                ("in-between", HintCheckpoints.MoreCheckpoints),
                ("mid-puzzle", HintCheckpoints.MidPuzzleCheckpoints)
            }
            .SelectMany(s => s.Item2.Select(c => (Set: s.Item1, Checkpoint: c)))
            .OrderBy(x => x.Checkpoint.Step)
            .ToList();

        // One pass through the game, stopping at every moment in walkthrough order.
        StartOver();
        var transcript = new List<string>();
        var moments = new List<Moment>();
        var played = 0;
        foreach (var (set, checkpoint) in all)
        {
            for (; played < checkpoint.Step; played++)
            {
                await DoWithSetup(steps[played].Command, steps[played].Setup);
                transcript.Add($"> {steps[played].Command}\n{LastResponse.Trim()}");
            }

            // A step with a setup hook is the walkthrough cheating (resetting the clock, say): the grader must know,
            // because a guide that sees the player's real state may rightly say "not until morning".
            var next = steps.Skip(checkpoint.Step).Take(NextCommandsShown)
                .Select(s => s.Setup is null ? s.Command : $"{s.Command} [the test first applies the god-mode hook '{s.Setup}', which a real player does not have]")
                .ToList();
            var situation = new PlanetfallOracleProvider().DescribeSituation(Context);
            var conversation = converse(checkpoint, string.Join("\n\n", transcript.TakeLast(15)));
            moments.Add(new Moment(set, checkpoint, Context.CurrentLocation.Name, situation, next, conversation));
        }

        // Grade everything.
        var graded = await Task.WhenAll(moments.Select(async m =>
        {
            var conversation = await m.Conversation;
            var grade = await Throttled(() => HintGrader.Judge(reference, m.Checkpoint.Puzzle, m.Situation, m.Next,
                conversation.Select(c => (c.Question, c.Answer)).ToList()));
            return (Moment: m, Conversation: conversation, Grade: grade);
        }));

        var log = new StringBuilder($"# Planetfall hints, graded — {system}\n\n");
        var summary = new StringBuilder("| set | GOOD | WEAK | WRONG | SPOILER |\n|---|---|---|---|---|\n");
        foreach (var set in graded.GroupBy(g => g.Moment.Set))
            summary.AppendLine($"| {set.Key} | {Count(set, Verdict.Good)} | {Count(set, Verdict.Weak)} | {Count(set, Verdict.Wrong)} | {Count(set, Verdict.Spoiler)} |");
        summary.AppendLine($"| **all {graded.Length}** | **{Count(graded, Verdict.Good)}** | {Count(graded, Verdict.Weak)} | {Count(graded, Verdict.Wrong)} | {Count(graded, Verdict.Spoiler)} |");
        log.AppendLine(summary.ToString());

        foreach (var g in graded.OrderBy(g => g.Grade.Verdict == Verdict.Good).ThenBy(g => g.Moment.Checkpoint.Step))
        {
            log.AppendLine($"## [{g.Grade.Verdict.ToString().ToUpperInvariant()}] {g.Moment.Checkpoint.Puzzle}");
            log.AppendLine($"_{g.Moment.Set} · step {g.Moment.Checkpoint.Step} · at **{g.Moment.Location}** · next: `{string.Join(", ", g.Moment.Next.Take(4))}`_  ");
            log.AppendLine($"_grader: {g.Grade.Reason}_\n");
            foreach (var (question, answer, label) in g.Conversation)
            {
                log.AppendLine($"**YOU:** {question}  ");
                log.AppendLine($"**GUIDE** `{label}`: {answer}\n");
            }
        }

        TestContext.Out.WriteLine(log.ToString());
        var outPath = Environment.GetEnvironmentVariable("HINT_EVAL_OUT");
        if (!string.IsNullOrWhiteSpace(outPath))
            await File.WriteAllTextAsync(outPath, log.ToString(), Encoding.UTF8);
    }

    private static int Count<T>(IEnumerable<(T, List<(string, string, string)>, Grade Grade)> graded, Verdict verdict) =>
        graded.Count(g => g.Grade.Verdict == verdict);

    private static readonly SemaphoreSlim Gate = new(4); // eight at once tripped the rate limit

    private static async Task<T> Throttled<T>(Func<Task<T>> work)
    {
        await Gate.WaitAsync();
        try
        {
            return await work();
        }
        finally
        {
            Gate.Release();
        }
    }
}
