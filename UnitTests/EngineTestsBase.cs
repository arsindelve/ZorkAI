using ChatLambda;
using CloudWatch;
using CloudWatch.Model;
using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Interface;
using ZorkOne;
using ZorkOne.GlobalCommand;

namespace UnitTests;

public class EngineTestsBase : EngineTestsBaseCommon<ZorkIContext>
{
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

        var takeAndDropParser = new Mock<IAITakeAndAndDropParser>();
        takeAndDropParser.Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string context) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
            });
        
        takeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string context) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
            });
        
        // Create a mock ParseConversation that mimics the old pattern behavior
        var mockParseConversation = CreateMockParseConversation();
        
        var engine = new GameEngine<ZorkI, ZorkIContext>(new ItemProcessorFactory(takeAndDropParser.Object),
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
        
        // Set up the mock to return predictable results that match old pattern expectations
        mockParseConversation.Setup(x => x.ParseAsync(It.IsAny<string>()))
            .ReturnsAsync((string input) =>
            {
                var lower = input.ToLowerInvariant();
                
                // Non-conversational commands should return (true, "")
                if (lower.StartsWith("throw") || lower.StartsWith("attack") || 
                    lower.StartsWith("examine") || lower.StartsWith("put") ||
                    lower.Contains("blabbedy"))
                {
                    return (true, "");
                }
                
                // Handle incomplete commands that should return (true, "") - no conversation
                if (lower.Trim() == "ask bob for" || lower.Trim() == "talk to")
                {
                    return (true, "");
                }
                
                // Simple pattern matching for test cases  
                if (lower.Contains("bob") || lower.Contains("bo"))
                {
                    if (lower == "bob, hello there") return (false, "hello there");
                    if (lower == "say to bob. 'hi'") return (false, "hi");
                    if (lower == "yell at bob get out") return (false, "get out");
                    if (lower == "yell at bob to go north") return (false, "to go north");
                    if (lower == "yell at bob \"go north\"") return (false, "go north");
                    if (lower == "yell to bob you stink") return (false, "you stink");
                    if (lower == "yell to bob \"go north\"") return (false, "go north");
                    if (lower == "yell to bob to go north") return (false, "to go north");
                    if (lower == "tell bob hello") return (false, "hello");
                    if (lower == "tell bob \"go north\"") return (false, "go north");
                    if (lower == "tell bob to go north") return (false, "go north");
                    if (lower == "say 'hi' to bob") return (false, "hi");
                    if (lower == "say \"hi\" to bob") return (false, "hi");
                    if (lower == "say hi to bob") return (false, "hi");
                    if (lower == "tell bob go north") return (false, "go north");
                    if (lower == "ask bob about the spaceship") return (false, "what about the spaceship?");
                    if (lower == "query bob for information about the mission") return (false, "can you tell me about the mission?");
                    if (lower == "ask bob for the key") return (false, "can I have the key?");
                    if (lower == "show golden key to bob") return (false, "look at this golden key");
                    if (lower == "interrogate bob") return (false, "Tell me everything you know.");
                    if (lower == "talk to bob") return (false, "hello");
                    if (lower == "talk to bo") return (false, "hello"); // Partial name match
                    if (lower == "speak with bob") return (false, "hello");
                    if (lower == "greet bob") return (false, "hello");
                    if (lower == "hello bob") return (false, "hello");
                    if (lower == "ask bob \"go north\"") return (false, "go north");
                    
                    // Edge cases - normalize multiple spaces
                    var normalized = System.Text.RegularExpressions.Regex.Replace(lower, @"\s+", " ").Trim();
                    if (normalized == "say to bob") return (false, ""); // Just spaces
                    if (normalized == "bob,") return (false, ""); // Comma with no message
                    
                    // Default for any bob-related input we don't recognize
                    return (false, "hello");
                }
                
                // No character mentioned or unrecognized pattern
                return (true, "");
            });
        
        return mockParseConversation;
    }
}