using Game.StaticCommand.Implementation;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace UnitTests;

/// <summary>
///     Represents a unit test appropriate implementation of the IIntentParser interface.
/// </summary>
internal class TestParser : IIntentParser
{
    private readonly string[] _allContainers;
    private readonly string[] _allNouns;
    private readonly string[] _verbs;

    public TestParser()
    {
        _verbs =
        [
            "take", "drop", "open", "close", "examine", "look", "eat", "press",
            "drink", "use", "count", "touch", "read", "turn", "wave", "move",
            "smell", "turn on", "turn off", "throw", "light", "rub", "kiss",
            "lower", "raise", "get", "inflate", "leave"
        ];

        _allNouns = Repository.GetNouns();
        _allContainers = Repository.GetContainers();

        IEnumerable<string> specialNouns =
        [
            "tree", "branches", "house", "lettering", "mirror", "match", "yellow button", "red button",
            "blue button", "brown button", "bolt", "bubble", "bodies", "gate", "lid", "switch", "slag", "engravings"
        ];

        _allNouns = _allNouns.Union(specialNouns).ToArray();
    }

    public Task<IntentBase> DetermineIntentType(string? input, string locationDescription, string sessionId)
    {
        if (string.IsNullOrEmpty(input))
            return Task.FromResult<IntentBase>(new NullIntent());

        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            if (input == direction.ToString() || input == $"go {direction.ToString()}")
                return Task.FromResult<IntentBase>(new MoveIntent { Direction = direction });

        if (input is "look" or "l")
            return Task.FromResult<IntentBase>(new GlobalCommandIntent { Command = new LookProcessor() });

        if (input is "inventory" or "i")
            return Task.FromResult<IntentBase>(new GlobalCommandIntent { Command = new InventoryProcessor() });

        if (input is "take all" or "get all")
            return Task.FromResult<IntentBase>(new GlobalCommandIntent { Command = new TakeEverythingProcessor() });

        if (input is "drop all")
            return Task.FromResult<IntentBase>(new GlobalCommandIntent { Command = new DropEverythingProcessor() });
        
        if (input is "cross the rainbow")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "rainbow",
                Verb = "cross",
            });
        
        if (input is "wait" or "z")
            return Task.FromResult<IntentBase>(new GlobalCommandIntent { Command = new WaitProcessor() });

        if (input == "tie rope to railing")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "rope",
                NounTwo = "railing",
                Preposition = "to",
                Verb = "tie",
                OriginalInput = "tie rope to railing"
            });
        
        
        if (input == "get in the boat")
            return Task.FromResult<IntentBase>(new EnterSubLocationIntent
            {
                Noun = "boat",
            });
        
        if (input == "get out of the boat")
            return Task.FromResult<IntentBase>(new ExitSubLocationIntent
            {
                NounOne = "boat",
            });
        
        if (input == "leave boat")
            return Task.FromResult<IntentBase>(new ExitSubLocationIntent
            {
                NounOne = "boat",
            });
        
        if (input == "inflate plastic with pump")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "boat",
                NounTwo = "pump",
                Preposition = "with",
                Verb = "inflate",
                OriginalInput = "inflate plastic with pump"
            });

        if (input == "turn bolt with wrench")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "bolt",
                NounTwo = "wrench",
                Preposition = "with",
                Verb = "turn",
                OriginalInput = "turn bolt with wrench"
            });
        
        if (input == "turn switch with screwdriver")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "switch",
                NounTwo = "screwdriver",
                Preposition = "with",
                Verb = "turn",
                OriginalInput = "turn switch with screwdriver"
            });

        if (input == "press the yellow button")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "the", Verb = "press", Noun = "yellow button", OriginalInput = "" });

        if (input == "press the brown button")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "the", Verb = "press", Noun = "brown button", OriginalInput = "" });

        if (input == "press the blue button")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "the", Verb = "press", Noun = "blue button", OriginalInput = "" });

        if (input == "press the red button")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "the", Verb = "press", Noun = "red button", OriginalInput = "" });

        if (input == "turn the lamp off")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "off", Verb = "turn", Noun = "lamp", OriginalInput = "" });

        if (input == "turn the lamp on")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "on", Verb = "turn", Noun = "lamp", OriginalInput = "turn the lamp on" });

        if (input == "turn on lantern")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "on", Verb = "turn", Noun = "lamp", OriginalInput = "turn the lamp on" });

        if (input == "turn on lamp")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "on", Verb = "turn", Noun = "lamp", OriginalInput = "turn the lamp on" });

        if (input == "turn off lantern")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "off", Verb = "turn", Noun = "lamp", OriginalInput = "turn the lamp on" });

        if (input == "light a match")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "a", Verb = "light", Noun = "match", OriginalInput = "" });

        if (input == "open trap door")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "", Verb = "open", Noun = "trap door", OriginalInput = "" });

        if (input == "light candles with match")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "candles",
                NounTwo = "match",
                Preposition = "with",
                Verb = "light",
                OriginalInput = "light candles with match"
            });
        
        if (input == "dig in sand with shovel")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "sand",
                NounTwo = "shovel",
                Preposition = "with",
                Verb = "dig",
                OriginalInput = "dig in sand with shovel"
            });

        if (input.StartsWith("put"))
        {
            var putWords = input.Split(" ");

            if (_allNouns.Contains(putWords[1]) && _allContainers.Contains(putWords[3]))
                return Task.FromResult<IntentBase>(new MultiNounIntent
                {
                    NounOne = putWords[1],
                    NounTwo = putWords[3],
                    Verb = "put",
                    Preposition = "in",
                    OriginalInput = input
                });
        }

        input = input.Replace("the ", "").Trim();

        var words = input.Split(" ");

        if (_verbs.Contains(words[0]) && _allNouns.Contains(words[1]))
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Verb = words[0],
                Noun = words[1],
                OriginalInput = input
            });

        return Task.FromResult<IntentBase>(new NullIntent());
    }
}