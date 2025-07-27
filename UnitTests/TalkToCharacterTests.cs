using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace UnitTests;

public class TalkToCharacterTests : EngineTestsBase
{
    private class TestTalker : ItemBase, ICanBeTalkedTo
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

    [Test]
    public async Task TellCharacterToDoSomething_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("tell bob to go north");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("go north");
    }

    [Test]
    public async Task AskCharacterAboutTopic_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("ask bob about the spaceship");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("what about the spaceship?");
    }

    [Test]
    public async Task QueryCharacterForInformation_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("query bob for information about the mission");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("can you tell me about the mission?");
    }

    [Test]
    public async Task AskCharacterForItem_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("ask bob for the key");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("can I have the key?");
    }

    [Test]
    public async Task TalkToCharacter_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("talk to bob");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("hello");
    }

    [Test]
    public async Task SpeakWithCharacter_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("speak with bob");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("hello");
    }

    [Test]
    public async Task GreetCharacter_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("greet bob");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("hello");
    }

    [Test]
    public async Task HelloCharacter_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("hello bob");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("hello");
    }

    [Test]
    public async Task ShowItemToCharacter_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("show golden key to bob");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("look at this golden key");
    }

    [Test]
    public async Task WhisperToCharacter_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("whisper to bob I found the treasure");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("(whispered) I found the treasure");
    }

    [Test]
    public async Task WhisperTextToCharacter_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("whisper 'the door is trapped' to bob");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("(whispered) 'the door is trapped'");
    }

    [Test]
    public async Task InterrogateCharacter_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("interrogate bob");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("Tell me everything you know.");
    }
}
