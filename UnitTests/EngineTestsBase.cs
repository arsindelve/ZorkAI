using ChatLambda;
using CloudWatch;
using CloudWatch.Model;
using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Interface;
using Moq;
using ZorkOne;
using ZorkOne.GlobalCommand;

namespace UnitTests;

public class EngineTestsBase : EngineTestsBaseCommon<ZorkIContext>
{
    protected Mock<IAITakeAndAndDropParser> TakeAndDropParser = new();

    /// <summary>
    ///     Returns an instance of the GameEngine class with the specified parser and client.
    ///     If parser is not provided, a default TestParser instance is used.
    /// </summary>
    /// <param name="parser">Optional parameter for the intent parser.</param>
    /// <returns>An instance of the GameEngine class.</returns>
    protected GameEngine<ZorkI, ZorkIContext> GetTarget(IIntentParser? parser = null)
    {
        Client = new Mock<IGenerationClient>();
        Parser = parser ?? new TestParser(new ZorkOneGlobalCommandFactory());

        Repository.Reset();

        TakeAndDropParser = new Mock<IAITakeAndAndDropParser>();
        TakeAndDropParser.Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string context) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
            });

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string context) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
            });

        // Create a mock ParseConversation that mimics the old pattern behavior
        var mockParseConversation = CreateMockParseConversation();

        var engine = new GameEngine<ZorkI, ZorkIContext>(new ItemProcessorFactory(TakeAndDropParser.Object),
            Parser, Client.Object, Mock.Of<ISecretsManager>(),
            Mock.Of<ICloudWatchLogger<TurnLog>>(), mockParseConversation.Object);
        
        engine.Context.Verbosity = Verbosity.Verbose;
        Repository.GetLocation<WestOfHouse>().Init();

        Context = engine.Context;
        
        return engine;
    }
    
    private static Mock<IParseConversation> CreateMockParseConversation()
    {
        var mockParseConversation = new Mock<IParseConversation>();
        
        // Set up the mock to return predictable results that match new isConversational pattern
        mockParseConversation.Setup(x => x.ParseAsync(It.IsAny<string>()))
            .ReturnsAsync((string input) =>
            {
                var lower = input.ToLowerInvariant();

                // Non-conversational commands should return (false, "") - not conversation
                if (lower.StartsWith("throw") || lower.StartsWith("attack") ||
                    lower.StartsWith("examine") || lower.StartsWith("put") ||
                    lower.Contains("blabbedy"))
                {
                    return (false, "");
                }

                // Handle incomplete commands that should return (false, "") - no conversation
                if (lower.Trim() == "ask bob for" || lower.Trim() == "talk to")
                {
                    return (false, "");
                }
                
                // Simple pattern matching for test cases - these ARE conversational
                if (lower.Contains("bob"))
                {
                    if (lower == "bob, hello there") return (true, "hello there");
                    if (lower == "say to bob. 'hi'") return (true, "hi");
                    if (lower == "yell at bob get out") return (true, "get out");
                    if (lower == "yell at bob to go north") return (true, "to go north");
                    if (lower == "yell at bob \"go north\"") return (true, "go north");
                    if (lower == "yell to bob you stink") return (true, "you stink");
                    if (lower == "yell to bob \"go north\"") return (true, "go north");
                    if (lower == "yell to bob to go north") return (true, "to go north");
                    if (lower == "tell bob hello") return (true, "hello");
                    if (lower == "tell bob \"go north\"") return (true, "go north");
                    if (lower == "tell bob to go north") return (true, "go north");
                    if (lower == "say 'hi' to bob") return (true, "hi");
                    if (lower == "say \"hi\" to bob") return (true, "hi");
                    if (lower == "say hi to bob") return (true, "hi");
                    if (lower == "tell bob go north") return (true, "go north");
                    if (lower == "ask bob about the spaceship") return (true, "what about the spaceship?");
                    if (lower == "query bob for information about the mission") return (true, "can you tell me about the mission?");
                    if (lower == "ask bob for the key") return (true, "can I have the key?");
                    if (lower == "show golden key to bob") return (true, "look at this golden key");
                    if (lower == "interrogate bob") return (true, "Tell me everything you know.");
                    if (lower == "talk to bob") return (true, "hello");
                    if (lower == "speak with bob") return (true, "hello");
                    if (lower == "greet bob") return (true, "hello");
                    if (lower == "hello bob") return (true, "hello");
                    if (lower == "ask bob \"go north\"") return (true, "go north");

                    // Edge cases - normalize multiple spaces
                    var normalized = System.Text.RegularExpressions.Regex.Replace(lower, @"\s+", " ").Trim();
                    if (normalized == "say to bob") return (true, ""); // Just spaces but conversational
                    if (normalized == "bob,") return (true, ""); // Comma with no message but conversational

                    // Default for any bob-related input we don't recognize - treat as conversational
                    return (true, "hello");
                }

                // No character mentioned or unrecognized pattern - not conversational
                return (false, "");
            });
        
        return mockParseConversation;
    }
}