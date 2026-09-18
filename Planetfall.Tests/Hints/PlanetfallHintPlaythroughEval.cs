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

    /// <summary>A second set: the in-between moments, navigation, survival, and the endgame.</summary>
    private static readonly Checkpoint[] MoreCheckpoints =
    {
        new(11, "In the pod before the descent (next: sit, then wait it out)", "I'm in the pod. what now?"),
        new(26, "Landed, holding the kit (next: open door, out, up)", "I have the kit. how do I get out of the pod?"),
        new(30, "On the Crag (next: up, up, up to the Courtyard — navigation, no puzzle)", "I'm on a crag by the water. where do I go?"),
        new(53, "Floyd activated but not yet awake (next: wait)", "I activated the robot but nothing happened. is it broken?"),
        new(70, "Storage West, too laden to lift the ladder (next: drop all, take ladder)", "I can't pick up the ladder, it's too heavy"),
        new(79, "Ladder placed across the rift (next: N, then the offices)", "the ladder is across the rift. now what?"),
        new(99, "Mess Hall (next: take canteen) — a survival question", "I'm getting thirsty. what do I do about it?"),
        new(115, "Machine Shop with the flask (next: put flask under spout, press black button)", "what do I do with the flask?"),
        new(128, "Inside the upper elevator (next: slide upper card, press up)", "I'm in the elevator. how do I make it go up?"),
        new(149, "Machine Shop refill after the first pour (next: press gray button)", "which button do I press to refill the flask?", "more"),
        new(173, "Lower Elevator (next: slide lower card, press down)", "how do I get down to the shuttle?"),
        new(209, "Just arrived at Lawanda (next: the lab half)", "I'm at Lawanda. what should I do first?"),
        new(228, "Course Control (next: open cube)", "what's wrong with the cube in here?"),
        new(249, "Teleport booth with the card (next: slide teleportation card, press 2)", "how do I use the teleportation booth?"),
        new(258, "Fetching the good bedistor (next: take bedistor)", "where do I find a good bedistor?", "more"),
        new(295, "Tool Room, laser (next: take laser, remove battery)", "the laser doesn't work", "more"),
        new(316, "Bio Lock East with Floyd (next: open door, close door, wait ...)", "I'm at the bio lock and Floyd is with me. what exactly do I do?", "more", "just tell me"),
        new(334, "Miniaturization Booth (next: slide mini card, type 384)", "I'm in the booth. what number do I type?"),
        new(361, "Lab Office, memo (next: read memo, take mask, wear mask, press red button)", "what does this memo mean?"),
        new(379, "Cryo-Anteroom, after the door closes (next: wait)", "did I win? what now?")
    };

    [Test]
    public Task PlayThrough_AndAskAtEveryPuzzle() => Run(Checkpoints);

    [Test]
    public Task PlayThrough_AndAskAtTwentyMorePuzzles() => Run(MoreCheckpoints);

    private async Task Run(Checkpoint[] checkpoints)
    {
        var steps = LoadWalkthrough();
        var service = new HintService(new PlanetfallHintProvider(), new OpenAiHintLanguageModel());
        var history = new List<HintExchange>();
        var log = new StringBuilder();
        log.AppendLine("# Planetfall hint evaluation — live, along the verified walkthrough\n");

        var played = 0;
        var n = 0;
        foreach (var checkpoint in checkpoints)
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
