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
        public bool WasCalled { get; set; }
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

    // A special talker that only responds to the name "dude" for negative tests
    private class TrackingTalker : ItemBase, ICanBeTalkedTo
    {
        public bool WasCalled { get; private set; }
        public string? ReceivedText { get; private set; }

        public override string[] NounsForMatching => ["dude"];

        public override string GenericDescription(ILocation? currentLocation) => string.Empty;

        public Task<string> OnBeingTalkedTo(string text, IContext context, IGenerationClient client)
        {
            WasCalled = true;
            ReceivedText = text;
            return Task.FromResult("I was called!");
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
    public async Task InterrogateCharacter_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse("interrogate bob");

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be("Tell me everything you know.");
    }

    #region Negative Tests

    [Test]
    public async Task TalkToCharacter_UnknownCharacter_ReturnsNull()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse("talk to stranger");

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse("because 'stranger' is not a valid character name");
    }

    [Test]
    public async Task AskCharacterForItem_UnknownCharacter_ReturnsNull()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse("ask alien for the key");

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse("because 'alien' is not a valid character name");
    }

    [Test]
    public async Task SayToCharacter_UnknownCharacter_ReturnsNull()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse("say to guard 'let me pass'");

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse("because 'guard' is not a valid character name");
    }

    [Test]
    public async Task NonConversationalCommand_WithValidCharacterName_ReturnsNull()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse("throw rock at bob");

        // Verify that no talker was called (because this is not a conversation command)
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse("because 'throw rock at' is not a conversation pattern");
    }

    [Test]
    public async Task NonConversationalCommand_WithUnknownCharacterName_ReturnsNull()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse("attack stranger");

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse("because 'attack' is not a conversation pattern");
    }

    [Test]
    public async Task EmptyRoom_TalkToValidCharacterName_ReturnsNull()
    {
        var engine = GetTarget();
        // Do not add talker to the room

        var result = await engine.GetResponse("talk to bob");

        // Result should not contain the talker's response
        result.Should().NotContain("ok");
    }

    [Test]
    public async Task CommaSeparatedFormat_UnknownCharacter_ReturnsNull()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse("stranger, hello there");

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse("because 'stranger' is not a valid character name");
    }

    [Test]
    public async Task TellCharacterToDoSomething_UnknownCharacter_ReturnsNull()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse("tell wizard to open the door");

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse("because 'wizard' is not a valid character name");
    }

    [Test]
    public async Task ShowItemToCharacter_UnknownCharacter_ReturnsNull()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse("show map to guide");

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse("because 'guide' is not a valid character name");
    }

    [Test]
    public async Task CharacterExistsButNoConversationCommand_ReturnsNull()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse("examine bob carefully");

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse("because 'examine' is not a conversation pattern");
    }

    [Test]
    public async Task WhisperToCharacter_UnknownCharacter_ReturnsNull()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse("whisper to ghost that we should leave");

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse("because 'ghost' is not a valid character name");
    }

    [Test]
    public async Task CharacterNameInMiddleOfCommand_NonTalkCommand_ReturnsNull()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse("put the ring near bob carefully");

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse("because this is not a conversation pattern");
    }

    #endregion
}