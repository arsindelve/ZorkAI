using System.Text;
using GameEngine;
using GameEngine.Hints.Oracle;
using Model.Hints;
using Model.Interface;
using Planetfall.Hints;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Tests.Walkthrough;
using ZorkAI.OpenAI;

namespace Planetfall.Tests.Hints.Oracle;

/// <summary>
///     A reading copy for a human judge: a couple of dozen situations — on and off the walkthrough's path — each
///     asked three times in a row, so the narrator's first answer (a question to ponder), second (the approach) and
///     third (the commands) sit side by side. No grader: the person reading it is the judge.
///     [Explicit]: live OpenAI (OPEN_AI_KEY). HINT_EVAL_OUT=path writes the markdown.
/// </summary>
[TestFixture]
[Explicit("Live OpenAI hint showcase — requires OPEN_AI_KEY")]
public class OracleShowcase : WalkthroughTestBase
{
    private const string More = "I still don't get it.";
    private const string Tell = "Just tell me exactly what to do.";

    /// <param name="Step">Replay the verified walkthrough up to (not including) this step.</param>
    /// <param name="Skip">Walkthrough steps the player never did (to get off the golden path).</param>
    /// <param name="Twist">A change to the world after the replay (to get further off it).</param>
    private sealed record Scene(string Title, string Setup, int Step, string[] Asks, int[]? Skip = null, Action? Twist = null);

    private static readonly int[] NeverActivatedFloyd = { 51, 52, 53 };

    private static readonly Scene[] Scenes =
    {
        new("The opening: scrubbing the deck", "Turn one aboard the Feinstein. Nothing has happened yet.", 0,
            new[] { "What am I supposed to be doing?", More, Tell }),
        new("A story question, far too early", "In the escape pod, moments after the explosion.", 12,
            new[] { "Why does the ship blow up?", "Come on, you must know.", "Just tell me." }),
        new("Landed", "The pod has come down. The player is still in the webbing.", 25,
            new[] { "We landed! How do I get out of here?", More, Tell }),
        new("Newly arrived, no goal in sight", "First steps into the complex; barely anything explored.", 34,
            new[] { "What am I supposed to be doing here?", More, Tell }),
        new("A dead end", "Inside the reactor elevator, which goes nowhere.", 42,
            new[] { "How do I get this elevator working?", "There's a slot and buttons. It must do something.", Tell }),
        new("What is this thing for?", "Just picked up the curved metal bar in the Tool Room.", 49,
            new[] { "I found a curved metal bar. What's it for?", More, Tell }),
        new("Never found Floyd", "Well into Kalamontee, across the rift — having walked straight past the Robot Shop without switching Floyd on.", 90,
            new[] { "I've been wandering around for ages. What now?", More, Tell }, Skip: NeverActivatedFloyd),
        new("Is the robot broken?", "Floyd is on and following the player around the complex.", 70,
            new[] { "This robot keeps wandering off and babbling nonsense. Is he broken? Should I turn him off?", "Is he dangerous?", "Does he actually do anything useful?" }),
        new("Something in the crevice", "Admin Corridor South, magnet in hand.", 60,
            new[] { "There's something shiny down in this crevice and I can't reach it.", More, Tell }),
        new("The rift", "Admin Corridor, carrying the ladder.", 76,
            new[] { "How do I cross the rift?", More, Tell }),
        new("The flask, before seeing the tower", "Machine Shop, empty flask in hand; has never been up the tower.", 115,
            new[] { "What do I do with this flask? There are coloured buttons.", More, Tell }),
        new("What should I be doing? (has explored)", "Elevator Lobby; most of Kalamontee explored, nothing repaired yet.", 123,
            new[] { "I've explored most of this place. What should I actually be doing?", More, Tell }),
        new("The spelling", "In the Comm Room, looking at the console's sign.", 135,
            new[] { "What does this sign say? It's gibberish.", "I tried. I still can't read it.", "Just translate it for me." }),
        new("Before the shuttle", "At the lower elevator, about to head for the shuttle. It is late in the day; the survival kit was left in Storage West.", 173,
            new[] { "Anything I should do before I get on the shuttle?", More, Tell }),
        new("Crashed the shuttle", "Aboard Alfie, about to set off.", 182,
            new[] { "I crashed the shuttle and died. What did I do wrong?", More, Tell }),
        new("The little door, without Floyd", "In the Repair Room at the little door — but Floyd was switched off and left behind in the Robot Shop, back in Kalamontee.", 215,
            new[] { "There's a tiny door here and I can see something through it. How do I get in?", More, Tell },
            Twist: () =>
            {
                var floyd = Repository.GetItem<Floyd>();
                floyd.IsOn = false; // switched off and left: he will not come on his own
                Repository.GetLocation<RobotShop>().ItemPlacedHere(floyd);
            }),
        new("Where did everybody go?", "In the Library lobby at Lawanda. Two repairs done; has not yet read the red spool or the printout.", 232,
            new[] { "I don't understand what happened here. Where did everybody go?", More, "Just tell me the story." }),
        new("The infirmary bed", "In Lawanda, after a restart.", 240,
            new[] { "I lay down on the bed in the infirmary and died. What on earth happened?", More, Tell }),
        new("The Bio Lock, before", "At the Bio Lock with Floyd. He has not gone in yet.", 316,
            new[] { "How do I get that card out of the bio lab? It's full of monsters.", "What's Floyd's plan? Is it safe?", Tell }),
        new("The Bio Lock, after", "Floyd has just died.", 322,
            new[] { "Floyd is dead. What do I do now?", More, Tell }),
        new("The microbe", "Miniaturized, on the strip. The microbe has just appeared.", 346,
            new[] { "A giant microbe just landed on the strip! Help!", "Shooting it isn't killing it.", Tell }),
        new("The mutants", "Mid-chase out of the Bio Lab, mutants behind.", 370,
            new[] { "The mutants are right behind me! How do I stop them?", "I tried closing the door and it doesn't work. What am I missing?", Tell })
    };

    [Test]
    public async Task WriteTheShowcase()
    {
        var steps = WalkthroughSource.Load("WalkthroughTestOne.cs");
        var provider = new PlanetfallOracleProvider();
        var running = new List<(Scene Scene, string Location, Task<List<(string Ask, string Answer)>> Conversation)>();

        foreach (var scene in Scenes)
        {
            // Every scene is its own game: replay to the moment, leave the path if the scene says so.
            StartOver();
            var transcript = new List<string>();
            for (var i = 0; i < scene.Step; i++)
            {
                if (scene.Skip?.Contains(i) == true) continue;
                await DoWithSetup(steps[i].Command, steps[i].Setup);
                transcript.Add($"> {steps[i].Command}\n{LastResponse.Trim()}");
            }

            scene.Twist?.Invoke();

            var frozen = new Frozen(provider.Persona, provider.GameKnowledge, provider.DescribeSituation(Context));
            var recent = string.Join("\n\n", transcript.TakeLast(15));
            running.Add((scene, Context.CurrentLocation.Name, Converse(frozen, Context, scene.Asks, recent)));
        }

        var md = new StringBuilder();
        md.AppendLine("# The narrator, three hints deep\n");
        md.AppendLine($"_Model: `{Environment.GetEnvironmentVariable("HINT_ORACLE_MODEL") ?? OpenAiHintOracle.DefaultModel}`. " +
                      "Each situation is a real game state (the verified walkthrough replayed to that moment, sometimes pushed off its path). " +
                      "The player asks three times: **A** is the first answer, **B** the second, **C** the third._\n");

        var n = 0;
        foreach (var (scene, location, conversation) in running)
        {
            var exchange = await conversation;
            md.AppendLine($"## {++n}. {scene.Title}");
            md.AppendLine($"_{scene.Setup} Standing in **{location}**._\n");
            for (var i = 0; i < exchange.Count; i++)
            {
                md.AppendLine($"**You:** {exchange[i].Ask}\n");
                md.AppendLine($"> **{(char)('A' + i)}.** {exchange[i].Answer.Replace("\n", "\n> ")}\n");
            }

            md.AppendLine("Your verdict: \n");
        }

        TestContext.Out.WriteLine(md.ToString());
        var outPath = Environment.GetEnvironmentVariable("HINT_EVAL_OUT");
        if (!string.IsNullOrWhiteSpace(outPath))
            await File.WriteAllTextAsync(outPath, md.ToString(), new UTF8Encoding(false));
    }

    private static readonly SemaphoreSlim Gate = new(8);

    private static async Task<List<(string Ask, string Answer)>> Converse(IOracleProvider frozen, IContext context, string[] asks, string transcript)
    {
        await Gate.WaitAsync();
        try
        {
            var service = new HintOracleService(frozen, new OpenAiHintOracle());
            var history = new List<HintExchange>();
            var log = new List<(string, string)>();
            foreach (var ask in asks)
            {
                var (text, isHint) = await service.GetHint(new OracleHintRequest(context, ask, history, transcript));
                if (isHint) history.Add(new HintExchange(ask, text));
                log.Add((ask, text));
            }

            return log;
        }
        finally
        {
            Gate.Release();
        }
    }

    private sealed class Frozen(HintPersona persona, string knowledge, string situation) : IOracleProvider
    {
        public HintPersona Persona => persona;
        public string GameKnowledge => knowledge;
        public string DescribeSituation(IContext state) => situation;
    }
}
