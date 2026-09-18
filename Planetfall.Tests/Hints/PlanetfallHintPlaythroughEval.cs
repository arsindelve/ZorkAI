using System.Text;
using System.Text.RegularExpressions;
using GameEngine.Hints;
using Model.Hints;
using Planetfall.Hints;
using Planetfall.Tests.Walkthrough;
using ZorkAI.OpenAI;

namespace Planetfall.Tests.Hints;

/// <summary>
///     LIVE hint evaluation across the whole game. Replays the verified walkthrough through the real engine
///     (with the walkthrough harness's deterministic mocks), stops just before each puzzle, and asks the way a
///     stuck player would — vaguely first, then "more" — threading the running conversation exactly as the
///     web client does (kind/topic/rung echoed, last 20 exchanges). Writes a transcript for a human to grade:
///     every hint should match what the walkthrough does NEXT, and never what it does later.
///     [Explicit]: real OpenAI (OPEN_AI_KEY). Set HINT_EVAL_OUT to also write the transcript to a file.
/// </summary>
[TestFixture]
[Explicit("Live OpenAI hint evaluation — requires OPEN_AI_KEY")]
public class PlanetfallHintPlaythroughEval : WalkthroughTestBase
{
    /// <summary>A puzzle checkpoint: stop BEFORE walkthrough step <see cref="Step" /> and ask.</summary>
    private sealed record Checkpoint(int Step, string Puzzle, params string[] Questions);

    private static readonly Checkpoint[] Checkpoints =
    {
        new(0, "Deck Nine, before the explosion (only 'wait' works)", "what should I do?", "more"),
        new(10, "Bulkhead open after the explosion (next: port)", "ok now what?!"),
        new(25, "Pod has landed (next: take kit, open door, out, up)", "we landed! now what?", "more"),
        new(42, "Reactor elevator — a dead end (next in walkthrough is just to leave)", "how do I use the reactor elevator?"),
        new(48, "Tool Room (next: take magnet)", "I'm stuck, what now?"),
        new(51, "Robot Shop (next: activate floyd)", "what do I do with these robots?"),
        new(60, "Crevice in Admin Corridor South (next: put magnet on crevice)", "there's a crevice here with something in it, what now?", "more"),
        new(64, "Padlocked door (next: unlock padlock with key)", "how do I open the padlocked door?"),
        new(76, "Admin Corridor, holding the ladder (next: drop, extend, place across rift)", "how do I cross the rift?", "more", "just tell me"),
        new(100, "Mess Hall slot (next: slide kitchen access card through slot)", "how do I get into the kitchen?"),
        new(135, "Tower Core, first pour (next: pour fluid into hole)", "what do I do in the tower?", "more"),
        new(163, "Tower Core, second pour (next: pour fluid into hole again)", "I poured the fluid in and the light went gray. now what?"),
        new(182, "Aboard Alfie (next: slide shuttle card, push lever, pull lever)", "how do I drive the shuttle?", "more"),
        new(215, "Repair Room with Floyd (next: floyd, go north / take board)", "how do I get the board from the repair room?"),
        new(220, "Planetary Defense (next: open panel, take second, put shiny in panel)", "how do I fix the planetary defense?"),
        new(232, "Library terminal — lore (tier 2 should be open here)", "why did the ship blow up?", "what is this place, really?"),
        new(312, "Bio Lock (next: open biolock door ... Floyd's sacrifice)", "how do I get the card from the bio lab?", "more"),
        new(340, "Strip Near Relay, miniaturized (next: set laser to 1, shoot speck)", "I'm tiny and there's a speck on the relay. what now?"),
        new(346, "The microbe on the strip (next: set laser to 2, shoot ×8, throw laser)", "a giant microbe landed on the strip! help!", "more"),
        new(367, "Lab Office after the red button (next: run for the cryo-elevator)", "the door's open and the mutants are coming, what do I do?")
    };

    [Test]
    public async Task PlayThrough_AndAskAtEveryPuzzle()
    {
        var steps = LoadWalkthrough();
        var service = new HintService(new PlanetfallHintProvider(), new OpenAiHintLanguageModel());
        var history = new List<HintExchange>();
        var log = new StringBuilder();
        log.AppendLine("# Planetfall hint evaluation — live, along the verified walkthrough\n");

        var played = 0;
        var n = 0;
        foreach (var checkpoint in Checkpoints)
        {
            // Advance the game to just before this puzzle.
            for (; played < checkpoint.Step; played++)
                await DoWithSetup(steps[played].Command, steps[played].Setup);

            n++;
            log.AppendLine($"## {n}. {checkpoint.Puzzle}");
            log.AppendLine($"_walkthrough step {checkpoint.Step}: `{steps[checkpoint.Step].Command}` · at **{Context.CurrentLocation.Name}**, score {Context.Score}, day {Context.Day}_\n");

            foreach (var question in checkpoint.Questions)
            {
                var r = await service.GetHint(new HintRequest("eval", Context, question, history.TakeLast(20).ToList()));
                if (r.IsHint)
                    history.Add(new HintExchange(question, r.Text, r.Topic, r.Rung, r.Kind.ToString()));

                log.AppendLine($"**YOU:** {question}  ");
                log.AppendLine($"`{r.Kind}` `{r.Topic ?? "-"}` `rung {r.Rung + 1}/{r.TotalRungs}`{(r.SoftLock != SoftLockKind.None ? $" `{r.SoftLock}`" : "")}  ");
                log.AppendLine($"**GUIDE:** {r.Text}\n");
            }
        }

        var text = log.ToString();
        TestContext.Out.WriteLine(text);
        var outPath = Environment.GetEnvironmentVariable("HINT_EVAL_OUT");
        if (!string.IsNullOrWhiteSpace(outPath))
            await File.WriteAllTextAsync(outPath, text, Encoding.UTF8);
    }

    private sealed record Step(string Command, string? Setup);

    /// <summary>The walkthrough's [TestCase] rows, in order, read from the source so this never drifts from it.</summary>
    private static List<Step> LoadWalkthrough()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Zork.sln"))) dir = dir.Parent;
        var path = Path.Combine(dir!.FullName, "Planetfall.Tests", "Walkthrough", "WalkthroughTestOne.cs");

        var rows = Regex.Matches(File.ReadAllText(path), "\\[TestCase\\(\"([^\"]*)\", *(null|\"([^\"]*)\")");
        return rows.Select(m => new Step(m.Groups[1].Value, m.Groups[3].Success ? m.Groups[3].Value : null)).ToList();
    }
}
