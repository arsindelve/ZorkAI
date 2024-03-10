using Game.StaticCommand.Implementation;
using Model;
using Model.Intent;
using ZorkOne;

namespace UnitTests;

public class EngineTestsBase
{
    protected Mock<IGenerationClient> Client = new();
    protected IIntentParser Parser = new IntentParser(); 
   
    protected GameEngine<ZorkI> GetTarget(IIntentParser? parser = null)
    {
        Client = new Mock<IGenerationClient>();

        if (parser == null)
        {
            Parser = Mock.Of<IIntentParser>();

            Mock<IIntentParser> mockParser = Mock.Get(Parser);

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                mockParser.Setup(p => p.DetermineIntentType(direction.ToString(), It.IsAny<string>()))
                    .ReturnsAsync(new MoveIntent { Direction = direction });
            }
            
            mockParser.Setup(p => p.DetermineIntentType("open mailbox", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "mailbox", Verb = "open", OriginalInput = "open mailbox" });
            
            mockParser.Setup(p => p.DetermineIntentType("take mailbox", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "mailbox", Verb = "take", OriginalInput = "take mailbox" });
            
            mockParser.Setup(p => p.DetermineIntentType("eat mailbox", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "mailbox", Verb = "eat", OriginalInput = "eat mailbox" });

            mockParser.Setup(p => p.DetermineIntentType("open sack", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "sack", Verb = "open", OriginalInput = "open sack" });

            mockParser.Setup(p => p.DetermineIntentType("drop sack", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "sack", Verb = "drop", OriginalInput = "drop sack" });

            mockParser.Setup(p => p.DetermineIntentType("take sack", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "sack", Verb = "take", OriginalInput = "take sack" });

            mockParser.Setup(p => p.DetermineIntentType("close sack", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "sack", Verb = "close", OriginalInput = "close sack" });

            mockParser.Setup(p => p.DetermineIntentType("examine sack", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "sack", Verb = "examine", OriginalInput = "examine sack" });

            mockParser.Setup(p => p.DetermineIntentType("open window", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "window", Verb = "open", OriginalInput = "open window" });

            mockParser.Setup(p => p.DetermineIntentType("close mailbox", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "mailbox", Verb = "close", OriginalInput = "close mailbox" });

            mockParser.Setup(p => p.DetermineIntentType("take leaflet", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "leaflet", Verb = "take", OriginalInput = "take leaflet" });

            mockParser.Setup(p => p.DetermineIntentType("take lunch", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "lunch", Verb = "take", OriginalInput = "take lunch" });

            mockParser.Setup(p => p.DetermineIntentType("take lantern", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "lantern", Verb = "take", OriginalInput = "take lantern" });

            mockParser.Setup(p => p.DetermineIntentType("drop lantern", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "lantern", Verb = "drop", OriginalInput = "drop lantern" });

            mockParser.Setup(p => p.DetermineIntentType("take garlic", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "garlic", Verb = "take", OriginalInput = "take garlic" });

            mockParser.Setup(p => p.DetermineIntentType("drop sword", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "sword", Verb = "drop", OriginalInput = "drop sword" });

            mockParser.Setup(p => p.DetermineIntentType("take sword", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "sword", Verb = "take", OriginalInput = "take sword" });

            mockParser.Setup(p => p.DetermineIntentType("drop leaflet", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Noun = "leaflet", Verb = "drop", OriginalInput = "drop leaflet" });

            mockParser.Setup(s => s.DetermineIntentType("read leaflet", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "read", Noun = "leaflet", OriginalInput = "read leaflet" });

            mockParser.Setup(s => s.DetermineIntentType("examine leaflet", It.IsAny<string>()))
                .ReturnsAsync(
                    new SimpleIntent { Verb = "examine", Noun = "leaflet", OriginalInput = "examine leaflet" });

            mockParser.Setup(s => s.DetermineIntentType("examine sword", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "examine", Noun = "sword", OriginalInput = "examine sword" });

            mockParser.Setup(s => s.DetermineIntentType("drop sword", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "drop", Noun = "sword", OriginalInput = "drop sword" });

            mockParser.Setup(s => s.DetermineIntentType("turn on lantern", It.IsAny<string>()))
                .ReturnsAsync(
                    new SimpleIntent { Verb = "turn on", Noun = "lantern", OriginalInput = "turn on lantern" });

            mockParser.Setup(s => s.DetermineIntentType("turn on lamp", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "turn on", Noun = "lamp", OriginalInput = "turn on lamp" });

            mockParser.Setup(s => s.DetermineIntentType("turn off lantern", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Verb = "turn off", Noun = "lantern", OriginalInput = "turn off lantern" });

            mockParser.Setup(s => s.DetermineIntentType("look", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "look", Noun = "lantern", OriginalInput = "look" });

            mockParser.Setup(s => s.DetermineIntentType("eat lunch", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "eat", Noun = "lunch", OriginalInput = "eat lunch" });
            
            mockParser.Setup(s => s.DetermineIntentType("throw lunch", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "throw", Noun = "lunch", OriginalInput = "throw lunch" });

            mockParser.Setup(s => s.DetermineIntentType("move rug", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "move", Noun = "rug", OriginalInput = "move rug" });

            mockParser.Setup(s => s.DetermineIntentType("open trap door", It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "open", Noun = "trap door", OriginalInput = "open trap door" });
            
            mockParser.Setup(s => s.DetermineIntentType("look", It.IsAny<string>()))
                .ReturnsAsync(new GlobalCommandIntent {Command = new LookProcessor()});
            
            mockParser.Setup(s => s.DetermineIntentType("i", It.IsAny<string>()))
                .ReturnsAsync(new GlobalCommandIntent {Command = new InventoryProcessor()});

        }
        else
        {
            Parser = parser;
        }

        Repository.Reset();
        var engine = new GameEngine<ZorkI>( Parser, Client.Object);
        return engine;
    }
}