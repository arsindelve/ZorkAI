using GameEngine.Item;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace UnitTests;

public class TalkToCharacterTests : EngineTestsBase
{
    internal class TestTalker : ItemBase, ICanBeTalkedTo
    {
        public bool WasCalled { get; private set; }
        public string? ReceivedText { get; private set; }

        public override string[] NounsForMatching => ["bob"];

        public override string GenericDescription(ILocation? currentLocation) => string.Empty;

        public Task<string> OnBeingTalkedTo(string text, IContext context, IGenerationClient client)
        {
            WasCalled = true;
            ReceivedText = text;
            return Task.FromResult("ok");
        }
    }

    [Test]
    public async Task CommaSeparatedFormat_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("bob, hello there");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("hello there");
    }

    [Test]
    public async Task SayVerbFormat_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("say to bob. 'hi'");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("hi");
    }

    [Test]
    public async Task YellAtFormat_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("yell at bob get out");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("get out");
    }

    [Test]
    public async Task YellToFormat_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("yell to bob you stink");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("you stink");
    }

    [Test]
    public async Task TellFormat_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("tell bob hello");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("hello");
    }

    [Test]
    public async Task SayTextToFormat_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("say 'hi' to bob");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("hi");
    }

    [Test]
    public async Task SayTextToFormat_DoubleQuotes_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("say \"hi\" to bob");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("hi");
    }

    [Test]
    public async Task SayTextToFormat_NoQuotes_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("say hi to bob");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("hi");
    }
}
