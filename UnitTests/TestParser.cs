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
            "take", "drop", "open", "close", "examine", "look", "eat", "press", "remove", "play", "shoot",
            "deactivate", "type", "key", "punch", "push", "pull", "burn", "set", "search", "empty",
            "drink", "use", "count", "touch", "read", "turn", "wave", "move", "ring", "activate", "search",
            "smell", "turn on", "turn off", "throw", "light", "rub", "kiss", "wind", "kick", "deflate",
            "lower", "raise", "get", "inflate", "leave", "unlock", "lock", "climb", "extend", "lift"
        ];

        _allNouns = Repository.GetNouns(gameName);
        _allContainers = Repository.GetContainers(gameName);

        IEnumerable<string> specialNouns =
        [
            "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "zero", "dial", "shelves", "pocket",
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "dude", "hello", "17", "seventeen", "slot", "lever", "label",
            "tree", "branches", "house", "lettering", "mirror", "match", "yellow button", "red button", "button", "medicine",
            "blue button", "brown button", "bolt", "bubble", "bodies", "gate", "lid", "switch", "slag", "engravings",
            "fromitz board", "board", "fromitz", "second fromitz board", "second board", "second",
            "second board", "second fromitz board", "second fromitz", "384"
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
        
        if (input is "press the button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "button",
                Verb = "press"
            });
        if (input is "press elevator button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "elevator button",
                Verb = "press"
            });
       
        if (input is "press up button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "up button",
                Verb = "press"
            });
        
        if (input is "press down button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "down button",
                Verb = "press"
            });
        
        if (input is "press blue button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "blue button",
                Verb = "press"
            });
        
        if (input is "press black button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "black button",
                Verb = "press"
            });
        
        if (input is "press gray button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "gray button",
                Verb = "press"
            });
        
        if (input is "press the one button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "one button",
                Verb = "press"
            });
        
        if (input is "press the two button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "two button",
                Verb = "press"
            });
        
        if (input is "press the three button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "three button",
                Verb = "press"
            });
        
        if (input is "press the 1 button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "1 button",
                Verb = "press"
            });
        
        if (input is "press the 2 button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "2 button",
                Verb = "press"
            });
        
        if (input is "press the tan button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "tan button",
                Verb = "press"
            });
        
        if (input is "press the brown button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "brown button",
                Verb = "press"
            });
        
        if (input is "press round button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "round button",
                Verb = "press"
            });
        
        if (input is "press square button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "square button",
                Verb = "press"
            });
        
        if (input is "press the beige button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "beige button",
                Verb = "press"
            });
        
        if (input is "press red button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "red button",
                Verb = "press"
            });
        
        if (input is "open red door")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "red door",
                Verb = "open"
            });
        
        if (input is "close red door")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "red door",
                Verb = "close"
            });
        
        if (input is "examine red door")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "red door",
                Verb = "examine"
            });
        
        if (input is "examine blue door")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "blue door",
                Verb = "examine"
            });
        
        if (input is "play with floyd")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "floyd",
                Verb = "play"
            });
        
        if (input is "open elevator door")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "elevator door",
                Verb = "open"
            });
        
        if (input is "open blue door")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "blue door",
                Verb = "open"
            });
        
        if (input is "close blue door")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "blue door",
                Verb = "close"
            });

        if (input is "open cell door")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "cell door",
                Verb = "open"
            });

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
        
        if (input is "sit on rug")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "rug",
                Verb = "sit",
                OriginalInput = "sit on rug"
            });
        
        if (input is "look under rug")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "rug",
                Verb = "look",
                OriginalInput = "look under rug"
            });

        if (input is "look under table")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "table",
                Verb = "look",
                OriginalInput = "look under table"
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
        
        if (input == "put flask under spout")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "flask",
                NounTwo = "spout",
                Preposition = "under",
                Verb = "put",
                OriginalInput = "put flask under spout"
            });
        
        if (input == "put trident in boat")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "trident",
                NounTwo = "boat",
                Preposition = "in",
                Verb = "put",
                OriginalInput = "put trident in boat"
            });
        
        if (input == "put bar in boat")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "bar",
                NounTwo = "boat",
                Preposition = "in",
                Verb = "put",
                OriginalInput = "put bar in boat"
            });
        
        if (input == "put emerald in boat")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "emerald",
                NounTwo = "boat",
                Preposition = "in",
                Verb = "put",
                OriginalInput = "put emerald in boat"
            });
        
        if (input == "put scarab in boat")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "scarab",
                NounTwo = "boat",
                Preposition = "in",
                Verb = "put",
                OriginalInput = "put scarab in boat"
            });

        if (input == "press button")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "button",
                Verb = "press"
            });
        
        if (input == "say temple")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "temple",
                Verb = "say"
            });
        
        if (input == "say treasure")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Noun = "treasure",
                Verb = "say"
            });

        if (input == "get in the boat")
            return Task.FromResult<IntentBase>(new EnterSubLocationIntent
            {
                Noun = "boat"
            });

        if (input == "enter the boat")
            return Task.FromResult<IntentBase>(new EnterSubLocationIntent
            {
                Noun = "boat"
            });

        if (input == "enter boat")
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

        if (input is "get out of bed" or "exit bed" or "leave bed" or "get out" or "stand" or "stand up" or "get up")
            return Task.FromResult<IntentBase>(new ExitSubLocationIntent
            {
                NounOne = "bed"
            });

        if (input == "set dial to 5651")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "dial",
                NounTwo = "5651",
                Preposition = "to",
                Verb = "set",
                OriginalInput = "set dial to 5651"
            });
        
        if (input == "set dial to 12")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "dial",
                NounTwo = "12",
                Preposition = "to",
                Verb = "set",
                OriginalInput = "set dial to 12"
            });
        
        if (input == "set dial to -12")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "dial",
                NounTwo = "-12",
                Preposition = "to",
                Verb = "set",
                OriginalInput = "set dial to -12"
            });
        
        if (input == "set dial to goo")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "dial",
                NounTwo = "goo",
                Preposition = "to",
                Verb = "set",
                OriginalInput = "set dial to goo"
            });
        
        if (input == "slide upper access card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "upper access card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide upper access card through slot"
            });
        
        if (input == "slide lower access card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "lower access card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide lower access card through slot"
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

        if (input == "slide miniaturization access card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "miniaturization access card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide miniaturization access card through slot"
            });

        if (input == "slide miniaturization card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "miniaturization card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide miniaturization card through slot"
            });

        if (input == "slide miniaturization through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "miniaturization",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide miniaturization through slot"
            });

        if (input == "slide mini card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "mini card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide mini card through slot"
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
        
        if (input == "slide teleportation access card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "teleportation access card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide teleportation access card through slot"
            });
        
        if (input == "slide teleportation card through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "teleportation card",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide access card through slot"
            });
        
        if (input == "slide teleportation through slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "teleportation",
                NounTwo = "slot",
                Preposition = "through",
                Verb = "slide",
                OriginalInput = "slide teleportation through slot"
            });
        
        if (input == "put upper elevator access card in slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "upper elevator access card",
                NounTwo = "slot",
                Preposition = "in",
                Verb = "put",
                OriginalInput = "put upper elevator access card in slot"
            });
        
        if (input == "put lower elevator access card in slot")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "lower elevator access card",
                NounTwo = "slot",
                Preposition = "in",
                Verb = "put",
                OriginalInput = "put lower elevator access card in slot"
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

        if (input == "examine crack")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Verb = "examine", Noun = "crack", OriginalInput = "examine crack" });

        if (input == "look through crack")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Verb = "look", Noun = "crack", OriginalInput = "look through crack" });

        if (input == "look through window")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Verb = "look", Noun = "window", OriginalInput = "look through window" });

        if (input == "examine equipment")
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Verb = "examine", Noun = "equipment", OriginalInput = "examine equipment" });

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

        if (input == "take fused with pliers")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "fused",
                NounTwo = "pliers",
                Preposition = "with",
                Verb = "take",
                OriginalInput = "take fused with pliers"
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
            
        if (input == "light matches with torch")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "matches",
                NounTwo = "torch",
                Preposition = "with",
                Verb = "light",
                OriginalInput = "light matches with torch"
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
        
        if (input == "pour fluid into hole")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "fluid",
                NounTwo = "hole",
                Preposition = "into",
                Verb = "pour",
                OriginalInput = "pour fluid into hole"
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
        
        if (input == "give the diary to floyd")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "diary",
                NounTwo = "floyd",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give the diary to floyd"
            });
        
        if (input == "give the key to floyd")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "key",
                NounTwo = "floyd",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give the key to floyd"
            });

        if (input == "give the plate to floyd")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "plate",
                NounTwo = "floyd",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give the plate to floyd"
            });

        if (input == "give plate to floyd")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "plate",
                NounTwo = "floyd",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give plate to floyd"
            });

        if (input == "give the breastplate to floyd")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "breastplate",
                NounTwo = "floyd",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give the breastplate to floyd"
            });

        if (input == "give breastplate to floyd")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "breastplate",
                NounTwo = "floyd",
                Preposition = "to",
                Verb = "give",
                OriginalInput = "give breastplate to floyd"
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
        
        if (input == "plug the leak with the gunk")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "leak",
                NounTwo = "gunk",
                Preposition = "with",
                Verb = "plug",
                OriginalInput = "plug the leak with the gunk"
            });
        
        if (input == "fix the plastic with the gunk")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "plastic",
                NounTwo = "gunk",
                Preposition = "with",
                Verb = "fix",
                OriginalInput = "fix the plastic with the gunk"
            });
        
        if (input == "apply the gunk to the water")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "gunk",
                NounTwo = "leak",
                Preposition = "to",
                Verb = "apply",
                OriginalInput = "apply the gunk to the water"
            });
        
                
        if (input == "put red spool in reader")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "red spool",
                NounTwo = "reader",
                Preposition = "in",
                Verb = "put",
                OriginalInput = "put red spool in reader"
            });
        
        if (input == "put green spool in reader")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "green spool",
                NounTwo = "reader",
                Preposition = "in",
                Verb = "put",
                OriginalInput = "put green spool in reader"
            });
        
        if (input == "set laser to 5")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "laser",
                NounTwo = "5",
                Preposition = "to",
                Verb = "set",
                OriginalInput = "set laser to 5"
            });
        
        if (input == "set laser to 6")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "laser",
                NounTwo = "6",
                Preposition = "to",
                Verb = "set",
                OriginalInput = "set laser to 6"
            });
        
        if (input == "set laser to 0")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "laser",
                NounTwo = "0",
                Preposition = "to",
                Verb = "set",
                OriginalInput = "set laser to 0"
            });
        
        
        if (input == "set laser to 90")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "laser",
                NounTwo = "90",
                Preposition = "to",
                Verb = "set",
                OriginalInput = "set laser to 90"
            });
        
        if (input == "set laser to bob")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "laser",
                NounTwo = "bob",
                Preposition = "to",
                Verb = "set",
                OriginalInput = "set laser to bob"
            });

        // Laser shooting commands
        if (input == "shoot laser with laser")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "laser",
                NounTwo = "laser",
                Preposition = "with",
                Verb = "shoot",
                OriginalInput = "shoot laser with laser"
            });

        if (input == "fire laser at laser")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "laser",
                NounTwo = "laser",
                Preposition = "at",
                Verb = "fire",
                OriginalInput = "fire laser at laser"
            });

        if (input == "shoot battery with laser")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "battery",
                NounTwo = "laser",
                Preposition = "with",
                Verb = "shoot",
                OriginalInput = "shoot battery with laser"
            });

        if (input == "shoot old battery with laser")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "old battery",
                NounTwo = "laser",
                Preposition = "with",
                Verb = "shoot",
                OriginalInput = "shoot old battery with laser"
            });

        if (input == "shoot flask with laser")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "flask",
                NounTwo = "laser",
                Preposition = "with",
                Verb = "shoot",
                OriginalInput = "shoot flask with laser"
            });

        if (input == "shoot magnet with laser")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "magnet",
                NounTwo = "laser",
                Preposition = "with",
                Verb = "shoot",
                OriginalInput = "shoot magnet with laser"
            });

        if (input == "shoot canteen with laser")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "canteen",
                NounTwo = "laser",
                Preposition = "with",
                Verb = "shoot",
                OriginalInput = "shoot canteen with laser"
            });

        if (input == "shoot floyd with laser")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "floyd",
                NounTwo = "laser",
                Preposition = "with",
                Verb = "shoot",
                OriginalInput = "shoot floyd with laser"
            });

        // "shoot laser at X" syntax - laser is noun one
        if (input == "shoot laser at flask")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "laser",
                NounTwo = "flask",
                Preposition = "at",
                Verb = "shoot",
                OriginalInput = "shoot laser at flask"
            });

        if (input == "shoot laser at floyd")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "laser",
                NounTwo = "floyd",
                Preposition = "at",
                Verb = "shoot",
                OriginalInput = "shoot laser at floyd"
            });

        if (input == "fire laser at flask")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "laser",
                NounTwo = "flask",
                Preposition = "at",
                Verb = "fire",
                OriginalInput = "fire laser at flask"
            });

        // Look at/into relay commands
        if (input == "look at relay")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Verb = "look at",
                Noun = "relay",
                OriginalInput = "look at relay"
            });

        if (input == "look into relay")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Verb = "look into",
                Noun = "relay",
                OriginalInput = "look into relay"
            });

        if (input == "look at microrelay")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Verb = "look at",
                Noun = "microrelay",
                OriginalInput = "look at microrelay"
            });

        if (input == "look into micro-relay")
            return Task.FromResult<IntentBase>(new SimpleIntent
            {
                Verb = "look into",
                Noun = "micro-relay",
                OriginalInput = "look into micro-relay"
            });

        if (input == "put good in cube")
            return Task.FromResult<IntentBase>(new MultiNounIntent
            {
                NounOne = "good bedistor",
                NounTwo = "cube",
                Preposition = "in",
                Verb = "put",
                OriginalInput = "put good in cube"
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

        // Handle multi-word goo nouns - must handle both adjective+noun and just "goo"
        if (input?.Contains("red goo") ?? false)
        {
            var verb = input.Split(' ')[0];
            if (_verbs.Contains(verb))
                return Task.FromResult<IntentBase>(new SimpleIntent
                {
                    Verb = verb,
                    Noun = "red goo",
                    OriginalInput = input
                });
        }

        if (input?.Contains("brown goo") ?? false)
        {
            var verb = input.Split(' ')[0];
            if (_verbs.Contains(verb))
                return Task.FromResult<IntentBase>(new SimpleIntent
                {
                    Verb = verb,
                    Noun = "brown goo",
                    OriginalInput = input
                });
        }

        if (input?.Contains("green goo") ?? false)
        {
            var verb = input.Split(' ')[0];
            if (_verbs.Contains(verb))
                return Task.FromResult<IntentBase>(new SimpleIntent
                {
                    Verb = verb,
                    Noun = "green goo",
                    OriginalInput = input
                });
        }

        // Handle just "goo" (defaults to first one found)
        if (input?.Split(' ').Contains("goo") ?? false)
        {
            var verb = input.Split(' ')[0];
            if (_verbs.Contains(verb))
                return Task.FromResult<IntentBase>(new SimpleIntent
                {
                    Verb = verb,
                    Noun = "goo",
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

    public override Task<string?> ResolvePronounsAsync(string input, string? lastInput, string? lastResponse)
    {
        // Simple test-mode pronoun resolution
        var lower = input.ToLowerInvariant();
        var lastInputLower = lastInput?.ToLowerInvariant() ?? string.Empty;
        var lastResponseLower = lastResponse?.ToLowerInvariant() ?? string.Empty;

        // "turn it on" after "take lamp" - pronoun refers to player's previous input
        if (lower.Contains("turn") && lower.Contains("it") && lastInputLower.Contains("lamp"))
            return Task.FromResult<string?>("turn lamp on");

        // "open it" after door mentioned in response
        if (lower.Contains("open it") && lastResponseLower.Contains("door"))
            return Task.FromResult<string?>("open door");

        // "open it" after bulkhead mentioned in response
        if (lower.Contains("open it") && lastResponseLower.Contains("bulkhead"))
            return Task.FromResult<string?>("open bulkhead");

        // No resolution needed
        return Task.FromResult<string?>(null);
    }
}
