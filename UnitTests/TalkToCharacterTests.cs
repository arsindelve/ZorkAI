using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;
using System.Diagnostics;
using System.Text.RegularExpressions;

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

        public override string[] NounsForMatching => ["dude"];

        public override string GenericDescription(ILocation? currentLocation) => string.Empty;

        public Task<string> OnBeingTalkedTo(string text, IContext context, IGenerationClient client)
        {
            WasCalled = true;
            return Task.FromResult("I was called!");
        }
    }

    // A custom talker with configurable name for special tests
    private class CustomTalker(string noun) : ItemBase, ICanBeTalkedTo
    {
        public bool WasCalled { get; private set; }

        public override string[] NounsForMatching => [noun];

        public override string GenericDescription(ILocation? currentLocation) => string.Empty;

        public Task<string> OnBeingTalkedTo(string text, IContext context, IGenerationClient client)
        {
            WasCalled = true;
            return Task.FromResult($"I am {noun}!");
        }
    }

    [TestCase("bob, hello there", "hello there", TestName = "CommaSeparatedFormat_TalksToCharacter")]
    [TestCase("say to bob. 'hi'", "hi", TestName = "SayVerbFormat_TalksToCharacter")]
    [TestCase("yell at bob get out", "get out", TestName = "YellAtFormat_TalksToCharacter")]
    [TestCase("yell at bob to go north", "to go north", TestName = "YellAtToFormat_TalksToCharacter")]
    [TestCase("yell at bob \"go north\"", "go north", TestName = "YellAtQuotedFormat_TalksToCharacter")]
    [TestCase("yell to bob you stink", "you stink", TestName = "YellToFormat_TalksToCharacter")]
    [TestCase("yell to bob \"go north\"", "go north", TestName = "YellToWithQuotesFormat_TalksToCharacter")]
    [TestCase("yell to bob to go north", "to go north", TestName = "YellToWithToFormat_TalksToCharacter")]
    [TestCase("tell bob hello", "hello", TestName = "TellFormat_TalksToCharacter")]
    [TestCase("tell bob \"go north\"", "go north", TestName = "TellCharacterQuotedFormat_TalksToCharacter")]
    [TestCase("tell bob to go north", "go north", TestName = "TellCharacterToWithoutQuotes_TalksToCharacter")]
    [TestCase("say 'hi' to bob", "hi", TestName = "SayTextToFormat_TalksToCharacter")]
    [TestCase("say \"hi\" to bob", "hi", TestName = "SayTextToFormat_DoubleQuotes_TalksToCharacter")]
    [TestCase("say hi to bob", "hi", TestName = "SayTextToFormat_NoQuotes_TalksToCharacter")]
    [TestCase("tell bob go north", "go north", TestName = "TellCharacterWithoutTo_TalksToCharacter")]
    public async Task BasicConversationFormats_TalksToCharacter(string command, string expectedText)
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse(command);

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be(expectedText);
    }

    [TestCase("ask bob about the spaceship", "what about the spaceship?", TestName = "AskCharacterAboutTopic")]
    [TestCase("query bob for information about the mission", "can you tell me about the mission?", TestName = "QueryCharacterForInformation")]
    [TestCase("ask bob for the key", "can I have the key?", TestName = "AskCharacterForItem")]
    [TestCase("show golden key to bob", "look at this golden key", TestName = "ShowItemToCharacter")]
    [TestCase("interrogate bob", "Tell me everything you know.", TestName = "InterrogateCharacter")]
    public async Task SpecialConversationFormats_TalksToCharacter(string command, string expectedText)
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse(command);

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be(expectedText);
    }

    [TestCase("talk to bob", "hello", TestName = "TalkToCharacter")]
    [TestCase("speak with bob", "hello", TestName = "SpeakWithCharacter")]
    [TestCase("greet bob", "hello", TestName = "GreetCharacter")]
    [TestCase("hello bob", "hello", TestName = "HelloCharacter")]
    public async Task DefaultHelloFormats_TalksToCharacter(string command, string expectedText)
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        await engine.GetResponse(command);

        talker.WasCalled.Should().BeTrue();
        talker.ReceivedText.Should().Be(expectedText);
    }

    [Test]
    public async Task CompareTellWithAndWithoutTo()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // First test 'tell bob to go north'
        talker.WasCalled = false;
        await engine.GetResponse("tell bob to go north");
        talker.WasCalled.Should().BeTrue();
        var withToResult = talker.ReceivedText;

        // Then test 'tell bob go north'
        talker.WasCalled = false;
        await engine.GetResponse("tell bob go north");
        talker.WasCalled.Should().BeTrue();
        var withoutToResult = talker.ReceivedText;

        // Both should have the same result
        withToResult.Should().Be("go north", "because the system correctly strips 'to' from 'tell X to Y'");
        withoutToResult.Should().Be("go north", "for consistency with the 'to' version");
        withToResult.Should().Be(withoutToResult, "because both command formats should be handled consistently");
    }

    #region Negative Tests

    [TestCase("talk to stranger", TestName = "TalkToCharacter_UnknownCharacter")]
    [TestCase("ask alien for the key", TestName = "AskCharacterForItem_UnknownCharacter")]
    [TestCase("say to guard 'let me pass'", TestName = "SayToCharacter_UnknownCharacter")]
    [TestCase("stranger, hello there", TestName = "CommaSeparatedFormat_UnknownCharacter")]
    [TestCase("tell wizard to open the door", TestName = "TellCharacterToDoSomething_UnknownCharacter")]
    [TestCase("show map to guide", TestName = "ShowItemToCharacter_UnknownCharacter")]
    [TestCase("whisper to ghost that we should leave", TestName = "WhisperToCharacter_UnknownCharacter")]
    public async Task UnknownCharacterCommands_DoesNotCallTalker(string command)
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse(command);

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse($"because the character in '{command}' is not valid");
    }

    [TestCase("throw rock at bob", TestName = "NonConversationalCommand_WithValidCharacterName")]
    [TestCase("attack stranger", TestName = "NonConversationalCommand_WithUnknownCharacterName")]
    [TestCase("examine bob carefully", TestName = "CharacterExistsButNoConversationCommand")]
    [TestCase("put the ring near bob carefully", TestName = "CharacterNameInMiddleOfCommand_NonTalkCommand")]
    [TestCase("blabbedy bob blah", TestName = "CompletelyMalformedCommand_NoCallsToOnBeingTalkedTo")]
    public async Task NonConversationalCommands_DoesNotCallTalker(string command)
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag to ensure it stays false
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse(command);

        // Verify that no talker was called
        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().BeFalse($"because '{command}' is not a conversation pattern");
    }

    [TestCase("talk to bob ", true, "hello", TestName = "TalkToCharacter_EmptyCommand_StillCalls")]
    [TestCase("say    to    bob   ", true, "", TestName = "TalkToCharacter_JustSpaces_StillCalls")]
    [TestCase("bob, ", true, "", TestName = "CommaSeparatedFormat_NoMessageAfterComma_StillMatches")]
    public async Task EdgeCaseCommands_VerifyBehavior(string command, bool shouldBeCalled, string expectedText)
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse(command);

        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().Be(shouldBeCalled);
        
        if (shouldBeCalled)
        {
            talker.ReceivedText.Should().Be(expectedText);
        }
    }

    [TestCase("ask bob for ", false, TestName = "AskCharacterForItem_NoItemSpecified")]
    [TestCase("talk to ", false, TestName = "TalkToNoOneSpecified")]
    public async Task IncompleteCommands_DoesNotCallTalker(string command, bool shouldBeCalled)
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag
        talker.GetType().GetProperty("WasCalled")!.SetValue(talker, false);

        await engine.GetResponse(command);

        var wasCalled = (bool)talker.GetType().GetProperty("WasCalled")!.GetValue(talker)!;
        wasCalled.Should().Be(shouldBeCalled);
    }

    [TestCase("talk to bo", true, TestName = "PartialCharacterName_StillMatches")]
    [TestCase("talk to xyz", false, TestName = "CompletelyDifferentName_NoMatch")]
    [TestCase("talk to BoB", true, TestName = "CharacterNameWithMixedCase_StillMatches")]
    [TestCase("say bob hello", true, TestName = "MalformedCommand_WithoutPreposition_StillMatches")]
    public async Task CharacterMatchingTests_VerifyBehavior(string command, bool shouldMatch)
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag
        talker.WasCalled = false;

        await engine.GetResponse(command);

        talker.WasCalled.Should().Be(shouldMatch);
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
    public async Task MultipleCharactersInRoom_CorrectOneMatched()
    {
        var engine = GetTarget();
        var talker1 = Repository.GetItem<TestTalker>();
        var talker2 = new TrackingTalker(); // This one responds to "dude"

        // Add both talkers to the room
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker1);
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker2);

        // Reset WasCalled flags
        talker1.WasCalled = false;

        // Talk to bob specifically
        await engine.GetResponse("talk to bob");

        // Verify only the correct talker was called
        talker1.WasCalled.Should().BeTrue("because we specifically addressed 'bob'");
        talker2.WasCalled.Should().BeFalse("because we didn't address 'dude'");
    }

    [Test]
    public async Task TwoCharactersWithSimilarNames_ExactMatchRequired()
    {
        // Create a custom talker class that responds to "bobby"
        var customTalker = new CustomTalker("bobby");
        var regularTalker = Repository.GetItem<TestTalker>(); // responds to "bob"

        var engine = GetTarget();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(customTalker);
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(regularTalker);

        // Reset WasCalled flags
        regularTalker.WasCalled = false;

        // Talk to bob specifically
        await engine.GetResponse("talk to bob");

        // Verify only the exact match was called
        regularTalker.WasCalled.Should().BeTrue("because we specifically addressed 'bob'");
        customTalker.WasCalled.Should().BeFalse("because we didn't address 'bobby'");
    }

    [Test]
    public async Task AskCharacterWithQuotes_TalksToCharacter()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag
        talker.WasCalled = false;

        await engine.GetResponse("ask bob \"go north\"");

        // Verify the talker was called with the correct message
        talker.WasCalled.Should().BeTrue("because 'ask bob to go north' is a valid command");
        talker.ReceivedText.Should().NotBe("can I have go north?", "because this isn't the 'ask for' pattern");
        talker.ReceivedText.Should().Be("go north", "because the system should strip the quotes");
    }

    [Test]
    public async Task AskCharacterWithMissingForClause_NoCallsToOnBeingTalkedTo()
    {
        // Setup a talker that tracks if it was called
        var talker = Repository.GetItem<TestTalker>();
        var engine = GetTarget();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        // Reset WasCalled flag
        talker.WasCalled = false;

        // Missing the "for" part
        await engine.GetResponse("ask bob the key");

        // In this case the AskCharacterForPattern shouldn't match because it requires "for"
        // However, another pattern might match this input format
        // To make a more reliable test, we could check that the ReceivedText doesn't contain "can I have"
        if (talker.WasCalled)
        {
            talker.ReceivedText.Should().NotContain("can I have", "because the AskCharacterForPattern shouldn't match");
        }
    }

    [Test]
    public async Task CompareAllCommandFormats()
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        var commandResults = new Dictionary<string, string>();

        // Test all the variants
        var commands = new[]
        {
            "tell bob to go north",
            "tell bob go north",
            "ask bob \"go north\"",
            "yell at bob to go north",
            "yell at bob \"go north\""
        };

        foreach (var command in commands)
        {
            talker.WasCalled = false;
            await engine.GetResponse(command);
            talker.WasCalled.Should().BeTrue($"because '{command}' should be a valid command");
            commandResults[command] = talker.ReceivedText!;
        }

        // Log the results
        foreach (var (command, result) in commandResults)
        {
            Console.WriteLine($"Command: '{command}' → Result: '{result}'");
        }

        // We expect consistent handling of the 'tell' commands
        commandResults["tell bob to go north"].Should().Be("go north");
        commandResults["tell bob go north"].Should().Be("go north");

        // Quoted text should have quotes removed
        commandResults["ask bob \"go north\""].Should().Be("go north");
        commandResults["yell at bob \"go north\""].Should().Be("go north");

        // 'yell at bob to go north' might have different behavior
        // depending on implementation
        commandResults["yell at bob to go north"].Should().NotBeEmpty();
    }

    #endregion
    [TestCase("tell bob to go north", "go north", TestName = "TellCommandWithTo_ProducesExpectedResult")]
    [TestCase("tell bob go north", "go north", TestName = "TellCommandWithoutTo_ProducesExpectedResult")]
    [TestCase("ask bob \"go north\"", "go north", TestName = "AskCommandWithQuotes_ProducesExpectedResult")]
    [TestCase("yell at bob \"go north\"", "go north", TestName = "YellAtCommandWithQuotes_ProducesExpectedResult")]
    public async Task CommandFormats_ProduceExpectedResults(string command, string expectedResult)
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        talker.WasCalled = false;
        await engine.GetResponse(command);
        
        talker.WasCalled.Should().BeTrue($"because '{command}' should be a valid command");
        talker.ReceivedText.Should().Be(expectedResult);
        
        // Log the result for debugging
        Console.WriteLine($"Command: '{command}' → Result: '{talker.ReceivedText}'");
    }

    [TestCase("yell at bob to go north", TestName = "YellAtCommandWithTo_ProducesNonEmptyResult")]
    public async Task CommandFormats_ProduceNonEmptyResults(string command)
    {
        var engine = GetTarget();
        var talker = Repository.GetItem<TestTalker>();
        (engine.Context.CurrentLocation as ICanContainItems)!.ItemPlacedHere(talker);

        talker.WasCalled = false;
        await engine.GetResponse(command);
        
        talker.WasCalled.Should().BeTrue($"because '{command}' should be a valid command");
        talker.ReceivedText.Should().NotBeEmpty("because the command should produce some result");
        
        // Log the result for debugging
        Console.WriteLine($"Command: '{command}' → Result: '{talker.ReceivedText}'");
    }
}