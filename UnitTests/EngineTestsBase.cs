using Game.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using ZorkOne;
using ZorkOne.GlobalCommand;

namespace UnitTests;

public class EngineTestsBase
{
    protected Mock<IGenerationClient> Client = new();
    protected IIntentParser Parser = new IntentParser(new ZorkOneGlobalCommandFactory());

    protected GameEngine<ZorkI, ZorkIContext> GetTarget(IIntentParser? parser = null)
    {
        Client = new Mock<IGenerationClient>();

        if (parser == null)
        {
            Parser = Mock.Of<IIntentParser>();

            var mockParser = Mock.Get(Parser);

            string[] verbs = { "take", "drop", "open", "close", "examine", "look", "eat", 
                "drink", "use", "count", "touch", "read", "turn", "wave", "move", 
                "smell", "turn on", "turn off", "throw", "light"};

            string[] nouns = Repository.GetNouns();

            foreach (var verb in verbs)
                foreach (var noun in nouns)
                {
                    mockParser.Setup(p => p.DetermineIntentType($"{verb} {noun}", It.IsAny<string>(), It.IsAny<string>()))
                            .ReturnsAsync(new SimpleIntent { Noun = noun, Verb = verb, OriginalInput = "" });

                    mockParser.Setup(p => p.DetermineIntentType($"{verb} the {noun}", It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync(new SimpleIntent { Noun = noun, Verb = verb, OriginalInput = "" });

                }
            
            mockParser.Setup(p => p.DetermineIntentType("inventory", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new GlobalCommandIntent { Command = new InventoryProcessor() });

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                mockParser.Setup(p =>
                        p.DetermineIntentType(direction.ToString(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new MoveIntent { Direction = direction });

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                mockParser.Setup(p =>
                        p.DetermineIntentType("go " + direction.ToString().ToLowerInvariant(), It.IsAny<string>(),
                            It.IsAny<string>()))
                    .ReturnsAsync(new MoveIntent { Direction = direction });

            mockParser.Setup(s => s.DetermineIntentType("throw lunch", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "throw", Noun = "lunch", OriginalInput = "throw lunch" });

            mockParser.Setup(s => s.DetermineIntentType("move rug", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "move", Noun = "rug", OriginalInput = "move rug" });

            mockParser.Setup(s => s.DetermineIntentType("open trap door", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "open", Noun = "trap door", OriginalInput = "open trap door" });

            mockParser.Setup(s => s.DetermineIntentType("take painting", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "take", Noun = "painting", OriginalInput = "take painting" });

            mockParser.Setup(s => s.DetermineIntentType("take knife", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "take", Noun = "knife", OriginalInput = "take knife" });

            mockParser.Setup(s => s.DetermineIntentType("take torch", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "take", Noun = "torch", OriginalInput = "take torch" });

            mockParser.Setup(s => s.DetermineIntentType("drop knife", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "drop", Noun = "knife", OriginalInput = "drop knife" });

            mockParser.Setup(s => s.DetermineIntentType("take rope", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "take", Noun = "rope", OriginalInput = "take rope" });

            mockParser.Setup(s => s.DetermineIntentType("take bell", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "take", Noun = "bell", OriginalInput = "take bell" });

            mockParser.Setup(s => s.DetermineIntentType("ring bell", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "ring", Noun = "bell", OriginalInput = "ring bell" });

            mockParser.Setup(s => s.DetermineIntentType("take water", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "take", Noun = "water", OriginalInput = "take water" });

            mockParser.Setup(s => s.DetermineIntentType("take coffin", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "take", Noun = "coffin", OriginalInput = "take coffin" });

            mockParser.Setup(s => s.DetermineIntentType("drop coffin", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "drop", Noun = "coffin", OriginalInput = "drop coffin" });

            mockParser.Setup(s => s.DetermineIntentType("drop figurine", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "drop", Noun = "figurine", OriginalInput = "drop figurine" });

            mockParser.Setup(s => s.DetermineIntentType("open coffin", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "open", Noun = "coffin", OriginalInput = "open coffin" });

            mockParser.Setup(s => s.DetermineIntentType("take sceptre", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "take", Noun = "sceptre", OriginalInput = "take sceptre" });

            mockParser.Setup(s => s.DetermineIntentType("take candles", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "take", Noun = "candles", OriginalInput = "take candles" });

            mockParser.Setup(s => s.DetermineIntentType("take book", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "take", Noun = "book", OriginalInput = "take book" });

            mockParser.Setup(s => s.DetermineIntentType("rub mirror", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "rub", Noun = "mirror", OriginalInput = "rub mirror" });

            mockParser.Setup(s => s.DetermineIntentType("drop book", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "drop", Noun = "book", OriginalInput = "drop book" });

            mockParser.Setup(s => s.DetermineIntentType("take skull", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "take", Noun = "skull", OriginalInput = "take skull" });

            mockParser.Setup(s => s.DetermineIntentType("open case", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent { Verb = "open", Noun = "case", OriginalInput = "open case" });

            mockParser.Setup(s => s.DetermineIntentType("look", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new GlobalCommandIntent { Command = new LookProcessor() });

            mockParser.Setup(s => s.DetermineIntentType("i", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new GlobalCommandIntent { Command = new InventoryProcessor() });

            mockParser.Setup(s => s.DetermineIntentType("wait", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new GlobalCommandIntent { Command = new WaitProcessor() });

            mockParser.Setup(s => s.DetermineIntentType("turn the lamp off", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "off", Verb = "turn", Noun = "lamp", OriginalInput = "turn the lamp off" });

            mockParser.Setup(s => s.DetermineIntentType("turn the lamp on", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "on", Verb = "turn", Noun = "lamp", OriginalInput = "turn the lamp on" });

            mockParser.Setup(s => s.DetermineIntentType("turn the lamp around", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "around", Verb = "turn", Noun = "lamp", OriginalInput = "turn the lamp around" });

            mockParser.Setup(s => s.DetermineIntentType("turn the lamp", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "turn", Noun = "lamp", OriginalInput = "turn the lamp" });

            mockParser.Setup(s => s.DetermineIntentType("press the red button", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "press", Noun = "red button", OriginalInput = "press the red button" });

            mockParser.Setup(s =>
                    s.DetermineIntentType("press the yellow button", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "press", Noun = "yellow button", OriginalInput = "press the yellow button" });

            mockParser.Setup(s =>
                    s.DetermineIntentType("press the brown button", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "press", Noun = "brown button", OriginalInput = "press the brown button" });

            mockParser.Setup(s => s.DetermineIntentType("move the leaves", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "move", Noun = "leaves", OriginalInput = "move the leaves" });

            mockParser.Setup(s => s.DetermineIntentType("take the leaves", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "take", Noun = "leaves", OriginalInput = "take the leaves" });

            mockParser.Setup(s => s.DetermineIntentType("take wrench", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "take", Noun = "wrench", OriginalInput = "take wrench" });

            mockParser.Setup(s => s.DetermineIntentType("drop wrench", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "drop", Noun = "wrench", OriginalInput = "drop wrench" });

            mockParser.Setup(s => s.DetermineIntentType("take screwdriver", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "take", Noun = "screwdriver", OriginalInput = "take screwdriver" });

            mockParser.Setup(s => s.DetermineIntentType("open the grating", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "open", Noun = "grating", OriginalInput = "open the grating" });

            mockParser.Setup(s => s.DetermineIntentType("count the leaves", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "count", Noun = "leaves", OriginalInput = "count the leaves" });

            mockParser.Setup(s => s.DetermineIntentType("take matches", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "take", Noun = "matches", OriginalInput = "take matches" });

            mockParser.Setup(s => s.DetermineIntentType("take figurine", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "take", Noun = "figurine", OriginalInput = "take figurine" });

            mockParser.Setup(s => s.DetermineIntentType("ring bell", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "ring", Noun = "bell", OriginalInput = "ring bell" });

            mockParser.Setup(s => s.DetermineIntentType("examine bell", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "ring", Noun = "bell", OriginalInput = "examine bell" });

            mockParser.Setup(s => s.DetermineIntentType("eat the spirits", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "eat", Noun = "spirits", OriginalInput = "eat the spirits" });

            mockParser.Setup(s => s.DetermineIntentType("kiss the spirits", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "kiss", Noun = "spirits", OriginalInput = "kiss the spirits" });

            mockParser.Setup(s => s.DetermineIntentType("take the spirits", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "take", Noun = "spirits", OriginalInput = "take the spirits" });

            mockParser.Setup(s => s.DetermineIntentType("examine the spirits", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "examine", Noun = "spirits", OriginalInput = "examine the spirits" });

            mockParser.Setup(s => s.DetermineIntentType("light a match", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "light", Noun = "match", OriginalInput = "light a match" });

            mockParser.Setup(s => s.DetermineIntentType("read book", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SimpleIntent
                    { Adverb = "", Verb = "read", Noun = "book", OriginalInput = "read book" });

            mockParser.Setup(s =>
                    s.DetermineIntentType("put painting inside case", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "painting",
                    NounTwo = "case",
                    Preposition = "inside",
                    Verb = "put",
                    OriginalInput = "put painting inside case"
                });

            mockParser.Setup(s =>
                    s.DetermineIntentType("put egg in case", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "egg",
                    NounTwo = "case",
                    Preposition = "in",
                    Verb = "put",
                    OriginalInput = "put egg in case"
                });

            mockParser.Setup(s =>
                    s.DetermineIntentType("put sceptre inside case", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "sceptre",
                    NounTwo = "case",
                    Preposition = "inside",
                    Verb = "put",
                    OriginalInput = "put sceptre inside case"
                });

            mockParser.Setup(s =>
                    s.DetermineIntentType("put leaflet inside case", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "leaflet",
                    NounTwo = "case",
                    Preposition = "inside",
                    Verb = "put",
                    OriginalInput = "put leaflet inside case"
                });

            mockParser.Setup(s => s.DetermineIntentType("put torch in basket", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "torch",
                    NounTwo = "basket",
                    Preposition = "in",
                    Verb = "put",
                    OriginalInput = "put torch in basket"
                });

            mockParser.Setup(s =>
                    s.DetermineIntentType("put screwdriver in basket", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "screwdriver",
                    NounTwo = "basket",
                    Preposition = "in",
                    Verb = "put",
                    OriginalInput = "put screwdriver in basket"
                });
            mockParser.Setup(s =>
                    s.DetermineIntentType("put coffin inside case", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "coffin",
                    NounTwo = "case",
                    Preposition = "inside",
                    Verb = "put",
                    OriginalInput = "put coffin inside case"
                });

            mockParser.Setup(s => s.DetermineIntentType("tie rope to railing", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "rope",
                    NounTwo = "railing",
                    Preposition = "to",
                    Verb = "tie",
                    OriginalInput = "tie rope to railing"
                });

            mockParser.Setup(s => s.DetermineIntentType("put gold in case", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "gold",
                    NounTwo = "case",
                    Preposition = "in",
                    Verb = "put",
                    OriginalInput = "put gold in case"
                });

            mockParser.Setup(s => s.DetermineIntentType("put sword in case", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "sword",
                    NounTwo = "case",
                    Preposition = "in",
                    Verb = "put",
                    OriginalInput = "put sword in case"
                });

            
            mockParser.Setup(
                    s => s.DetermineIntentType("turn bolt with wrench", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "bolt",
                    NounTwo = "wrench",
                    Preposition = "with",
                    Verb = "turn",
                    OriginalInput = "turn bolt with wrench"
                });

            mockParser.Setup(s =>
                    s.DetermineIntentType("light candles with match", It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MultiNounIntent
                {
                    NounOne = "candles",
                    NounTwo = "match",
                    Preposition = "with",
                    Verb = "light",
                    OriginalInput = "light candles with match"
                });
        }
        else
        {
            Parser = parser;
        }

        Repository.Reset();

        var engine = new GameEngine<ZorkI, ZorkIContext>(Parser, Client.Object);
        Repository.GetLocation<WestOfHouse>().Init();

        return engine;
    }
}