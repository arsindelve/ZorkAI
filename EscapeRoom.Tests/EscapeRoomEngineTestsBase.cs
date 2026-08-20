using ChatLambda;
using CloudWatch;
using CloudWatch.Model;
using EscapeRoom.GlobalCommand;
using EscapeRoom.Location;
using GameEngine;
using GameEngine.Item;
using Model;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Interface;

namespace EscapeRoom.Tests;

public class EscapeRoomEngineTestsBase : EngineTestsBaseCommon<EscapeRoomContext>
{
    protected Mock<IAgenticActionParser> AgenticActionParser = new();
    protected Mock<IAITakeAndAndDropParser> TakeAndDropParser = new();

    protected GameEngine<EscapeRoomGame, EscapeRoomContext> GetTarget(IIntentParser? parser = null)
    {
        Client = new Mock<IGenerationClient>();
        Parser = parser ?? new TestParser(new EscapeRoomGlobalCommandFactory(), "EscapeRoom");

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

        // Inert agentic-narrator default (issue #136): no narration, no tool calls, so the engines
        // keep the plain fall-through behavior unless a test stubs a real decision.
        AgenticActionParser = new Mock<IAgenticActionParser>();
        AgenticActionParser.Setup(s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AgenticActionResult(string.Empty, []));

        var mockParseConversation = new Mock<IParseConversation>();
        mockParseConversation.Setup(x => x.ParseAsync(It.IsAny<string>()))
            .ReturnsAsync((false, ""));

        var engine = new GameEngine<EscapeRoomGame, EscapeRoomContext>(
            new ItemProcessorFactory(TakeAndDropParser.Object, AgenticActionParser.Object),
            Parser,
            Client.Object,
            Mock.Of<ISecretsManager>(),
            Mock.Of<ICloudWatchLogger<TurnLog>>(),
            mockParseConversation.Object);

        engine.Context.Verbosity = Verbosity.Verbose;
        Repository.GetLocation<Reception>().Init();

        Context = engine.Context;

        return engine;
    }
}
