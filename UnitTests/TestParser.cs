using GameEngine;
using GameEngine.StaticCommand.Implementation;
using Model.AIParsing;
using Model.Intent;
using Model.Interface;

namespace UnitTests;

/// <summary>
///     Represents a unit test appropriate implementation of the IIntentParser interface.
/// </summary>
public class TestParser : IntentParser
{
    private readonly string[] _allContainers;
    private readonly string[] _allNouns;
    private readonly string[] _verbs;

    public TestParser(IGlobalCommandFactory gameSpecificCommandFactory, string gameName = "ZorkOne") : base(
        Mock.Of<IAIParser>(), gameSpecificCommandFactory)
    {
        _verbs =
        [
            "take", "drop", "open", "close", "examine", "look", "eat", "press", "remove",
            "deactivate", "type", "key", "punch", "push",
            "drink", "use", "count", "touch", "read", "turn", "wave", "move", "ring", "activate",
            "smell", "turn on", "turn off", "throw", "light", "rub", "kiss", "wind",
            "lower", "raise", "get", "inflate", "leave", "unlock", "lock", "climb", "extend"
        ];

        _allNouns = Repository.GetNouns(gameName);
        _allContainers = Repository.GetContainers(gameName);

        IEnumerable<string> specialNouns =
        [
            "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "zero",
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "dude", "hello", "17", "seventeen",
            "tree", "branches", "house", "lettering", "mirror", "match", "yellow button", "red button", "button",
            "blue button", "brown button", "bolt", "bubble", "bodies", "gate", "lid", "switch", "slag", "engravings"
        ];

        _allNouns = _allNouns.Union(specialNouns).ToArray();
    }

    public override Task<IntentBase> DetermineComplexIntentType(string? input, string locationDescription,
        string sessionId)
    {
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
                Verb = "cross"
            });

        if (input is "drop the access card")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "access card",
                Verb = "drop"
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

        if (input == "place ladder across rift")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "ladder",
                NounTwo = "rift",
                Preposition = "across",
                Verb = "place",
                OriginalInput = "place ladder across rift"
            });

        if (input == "press button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "button",
                Verb = "press"
            });

        if (input == "get in the boat")
            return Task.FromResult<IntentBase>(new EnterSubLocationIntent
            {
                Noun = "boat"
            });


        if (input == "get out of the boat")
            return Task.FromResult<IntentBase>(new ExitSubLocationIntent
            {
                NounOne = "boat"
            });

        if (input == "leave boat")
            return Task.FromResult<IntentBase>(new ExitSubLocationIntent
            {
                NounOne = "boat"
            });

        if (input == "slide kitchen access card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "kitchen access card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide kitchen access card through slot"
            });

        if (input == "slide access card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "access card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide access card through slot"
            });

        if (input == "slide shuttle access card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "shuttle access card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide shuttle access card through slot"
            });

        if (input == "slide upper elevator access card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "upper elevator access card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide upper elevator access card through slot"
            });

        if (input == "slide card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide card through slot"
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

        if (input == "unlock grate with the key")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                Verb = "unlock",
                NounOne = "grate",
                NounTwo = "key",
                Preposition = "with",
                OriginalInput = "unlock grate with the key"
            });

        if (input == "lock the grate with the key")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                Verb = "lock",
                NounOne = "grate",
                NounTwo = "key",
                Preposition = "with",
                OriginalInput = "lock the grate with the key"
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

        if (input == "open the egg with the screwdriver")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "egg",
                NounTwo = "screwdriver",
                Preposition = "with",
                Verb = "open",
                OriginalInput = "open the egg with the screwdriver"
            });

        if (input == "open the egg with the knife")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "egg",
                NounTwo = "knife",
                Preposition = "with",
                Verb = "open",
                OriginalInput = "open the egg with the knife"
            });

        if (input == "open the egg with the sword")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "egg",
                NounTwo = "sword",
                Preposition = "with",
                Verb = "open",
                OriginalInput = "open the egg with the sword"
            });

        if (input == "press the yellow button")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "the", Verb = "press", Noun = "yellow button", OriginalInput = "" });

        if (input == "press yellow")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "the", Verb = "press", Noun = "yellow button", OriginalInput = "" });

        if (input == "press yellow button")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Adverb = "", Verb = "press", Noun = "yellow", OriginalInput = "" });

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

        if (input == "put magnet on crevice")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "magnet",
                NounTwo = "crevice",
                Preposition = "on",
                Verb = "put",
                OriginalInput = "put magnet on crevice"
            });

        if (input == "unlock padlock with key")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "padlock",
                NounTwo = "key",
                Preposition = "with",
                Verb = "unlock",
                OriginalInput = "unlock padlock with key"
            });

        if (input == "light candles with torch")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "candles",
                NounTwo = "torch",
                Preposition = "with",
                Verb = "light",
                OriginalInput = "light candles with torch"
            });

        if (input == "kill the cyclops with the sword")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "cyclops",
                NounTwo = "sword",
                Preposition = "with",
                Verb = "kill",
                OriginalInput = "kill the cyclops with the sword"
            });
        
        if (input == "kill the troll with the sword")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "troll",
                NounTwo = "sword",
                Preposition = "with",
                Verb = "kill",
                OriginalInput = "kill the troll with the sword"
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

        if (input == "offer the cyclops the lunch")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "cyclops",
                NounTwo = "lunch",
                Preposition = "",
                Verb = "offer",
                OriginalInput = "offer the cyclops the lunch"
            });

        if (input == "give the lunch to the cyclops")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "lunch",
                NounTwo = "cyclops",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give the lunch to the cyclops"
            });

        if (input == "give the bottle to the cyclops")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "bottle",
                NounTwo = "cyclops",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give the bottle to the cyclops"
            });

        if (input == "give the garlic to the cyclops")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "garlic",
                NounTwo = "cyclops",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give the garlic to the cyclops"
            });

        if (input == "give the axe to the cyclops")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "axe",
                NounTwo = "cyclops",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give the axe to the cyclops"
            });
        
        if (input == "give the egg to the thief")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "egg",
                NounTwo = "thief",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give the egg to the thief"
            });
        
        if (input == "give the trident to the thief")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "trident",
                NounTwo = "thief",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give the trident to the thief"
            });
        
        if (input == "give the sceptre to the thief")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "sceptre",
                NounTwo = "thief",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give the sceptre to the thief"
            });
        
        if (input == "kill thief with sword")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "thief",
                NounTwo = "sword",
                Preposition = "with",
                Verb = "kill",
                OriginalInput = "kill thief with sword"
            });
        
        if (input == "kill thief with knife")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "thief",
                NounTwo = "knife",
                Preposition = "with",
                Verb = "kill",
                OriginalInput = "kill thief with knife"
            });

        if (input?.StartsWith("put") ?? false)
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

        input = input?.Replace("the ", "").Trim();

        var words = input?.Split(" ");

        if (_verbs.Contains(words?[0]) && _allNouns.Contains(words?[1]))
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Verb = words?[0] ?? string.Empty,
                Noun = words?[1],
                OriginalInput = input
            });

        return Task.FromResult<IntentBase>(new NullIntent());
    }
}