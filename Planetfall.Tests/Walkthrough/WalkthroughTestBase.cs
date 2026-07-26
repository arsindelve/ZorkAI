using System.Diagnostics;
using System.Text;
using ChatLambda;
using DynamoDb;
using FluentAssertions;
using GameEngine;
using JetBrains.Annotations;
using Model.Interface;
using Moq;
using Planetfall.AI;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.BioLab;
using Planetfall.Location.Feinstein;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Tests.Walkthrough;

public abstract class WalkthroughTestBase : EngineTestsBase
{
    private readonly DynamoDbSessionRepository _database = new();
    private GameEngine<PlanetfallGame, PlanetfallContext> _target;
    private Mock<IRandomChooser> _floydChooser;
    private Mock<IRandomChooser> _laserChooser;
    private Mock<IRandomChooser> _chaseChooser;
    private Mock<IRandomChooser> _deckNineChooser;
    private Mock<IRandomChooser> _escapePodChooser;
    private Mock<IRandomChooser> _adminCorridorSouthChooser;
    private Mock<IChatWithFloyd> _chatWithFloyd;

    /// <summary>
    ///     The one random beat in the escape-pod sequence. When the Feinstein blows apart and the player
    ///     is NOT strapped into the safety webbing, a one-in-five roll decides between an instant
    ///     head-first death and a survivable bruising. Walkthroughs that ride the explosion out in the
    ///     webbing never reach the roll at all (the check short-circuits on being in the web), so this
    ///     defaults to the survivable branch; a walkthrough documenting the head-first death sets it true.
    /// </summary>
    protected bool ThrownAgainstTheBulkheadIsFatal { get; set; }

    [OneTimeSetUp]
    public void Init()
    {
        _target = GetTarget();

        // Set up ParseConversation to recognize "floyd, go north" as conversation
        ParseConversationMock.Setup(x => x.ParseAsync("floyd, go north"))
            .ReturnsAsync((true, "go north")); // (true, "go north") means it IS conversational, rewritten to "go north"

        _floydChooser = new Mock<IRandomChooser>();
        // Force Floyd's lower-elevator-card reveal: the daemon now rolls RollDice(100) against a day-keyed
        // chance (#222), so a roll of 1 lands inside every day's window (including Day 1's small chance).
        _floydChooser.Setup(s => s.RollDice(100)).Returns(1);

        // Prevent Floyd from wandering during walkthrough tests
        _floydChooser.Setup(s => s.RollDiceSuccess(5)).Returns(false);  // Don't stop following when player moves
        _floydChooser.Setup(s => s.RollDiceSuccess(20)).Returns(false); // Don't spontaneously wander

        // Laser always hits the speck (roll 1 is always <= hitChance)
        _laserChooser = new Mock<IRandomChooser>();
        _laserChooser.Setup(s => s.RollDice(100)).Returns(1);

        // Chase scene always uses the first message for deterministic tests
        _chaseChooser = new Mock<IRandomChooser>();
        _chaseChooser.Setup(s => s.Choose(It.IsAny<List<string>>()))
            .Returns("The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms! ");

        // Deck Nine rolls a d6 every turn between moves 2 and 6 to decide whether the ambassador (1)
        // or Blather (2) wanders in. Any other value means nobody does, which keeps the ten turns
        // before the explosion byte-identical on every run.
        _deckNineChooser = new Mock<IRandomChooser>();
        _deckNineChooser.Setup(s => s.RollDice(6)).Returns(3);

        // See ThrownAgainstTheBulkheadIsFatal. Read through a lambda so a fixture can choose the
        // branch it documents from its own setup, after this one-time init has already run.
        _escapePodChooser = new Mock<IRandomChooser>();
        _escapePodChooser.Setup(s => s.RollDiceSuccess(5)).Returns(() => ThrownAgainstTheBulkheadIsFatal);

        // Admin Corridor South drops a one-in-three "glint of light" hint into any turn spent there.
        // Suppress it so it can't appear mid-assertion on some runs and not others.
        _adminCorridorSouthChooser = new Mock<IRandomChooser>();
        _adminCorridorSouthChooser.Setup(s => s.RollDiceSuccess(3)).Returns(false);

        _chatWithFloyd = new Mock<IChatWithFloyd>();
        _chatWithFloyd.Setup(s => s.AskFloydAsync("go north")).ReturnsAsync(new CompanionResponse(
            "Floyd's response",
            new CompanionMetadata("GoSomewhere", new Dictionary<string, object> { { "direction", "north" } })
        ));
        _chatWithFloyd.Setup(s => s.AskFloydAsync("take board")).ReturnsAsync(new CompanionResponse(
            "Floyd's response",
            new CompanionMetadata("PickUp", new Dictionary<string, object> { { "object", "board" } })
        ));

        ParseConversationMock.Setup(x => x.ParseAsync("floyd, take board")).Returns(Task.FromResult((true, "take board")));
        ParseConversationMock.Setup(x => x.ParseAsync("floyd, go north")).Returns(Task.FromResult((true, "go north")));
    }

    protected void InvokeGodMode(string setup)
    {
        // Ooooooh! Reflection!!
        var method = GetType().GetMethod(setup);
        if (method == null)
            throw new ArgumentException("Method " + setup + " doesn't exist");

        // Invoke the method on the current instance
        method.Invoke(this, null);
    }

    [UsedImplicitly]
    public void ResetTime()
    {
        Repository.GetItem<Chronometer>().CurrentTime = 2000;
    }

    protected async Task Do(string input, params string[] outputs)
    {
        var floyd = Repository.GetItem<Floyd>();
        floyd.Chooser = _floydChooser.Object;
        floyd.ChatWithFloyd = _chatWithFloyd.Object;

        var laser = Repository.GetItem<Laser>();
        laser.Chooser = _laserChooser.Object;

        var chaseManager = Repository.GetItem<ChaseSceneManager>();
        chaseManager.Chooser = _chaseChooser.Object;

        Repository.GetLocation<DeckNine>().Chooser = _deckNineChooser.Object;
        Repository.GetLocation<EscapePod>().Chooser = _escapePodChooser.Object;
        Repository.GetLocation<AdminCorridorSouth>().Chooser = _adminCorridorSouthChooser.Object;

        var result = await _target.GetResponse(input);
        if (Debugger.IsAttached)
        {
            Console.WriteLine(result);
            var sessionId = Environment.MachineName + "8";
            var bytesToEncode = Encoding.UTF8.GetBytes(_target.Context.Engine!.SaveGame());
            var encodedText = Convert.ToBase64String(bytesToEncode);
            await _database.WriteSessionState(sessionId, encodedText, _target.SessionTableName);
        }

        foreach (var output in outputs)
            result.Should().Contain(output);
    }

    protected async Task DoWithSetup(string input, string? setup, params string[] outputs)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, outputs);
    }
}