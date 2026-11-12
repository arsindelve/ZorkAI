using Model.Movement;
using static UnitTests.Parser.Meaning;

namespace UnitTests.Parser;

public static partial class ParserCorpus
{
    private static IEnumerable<ParserCase> ZorkOne()
    {
        const string game = "ZorkOne";
        ParserCase At(string location, string input, Meaning meaning) => meaning.Build(game, location, input);

        return
        [
            // ---------------------------------------------------------------- answered before any parsing
            Heading(game, "north", Direction.N),
            Heading(game, "n", Direction.N),
            Heading(game, "south", Direction.S),
            Heading(game, "up", Direction.Up),
            Heading(game, "d", Direction.Down),
            Heading(game, "ne", Direction.NE),
            Heading(game, "go north", Direction.N),
            Heading(game, "walk north", Direction.N),
            Heading(game, "go up", Direction.Up),
            Heading(game, "climb the staircase", Direction.Up),
            Heading(game, "go up the staircase", Direction.Up),
            Heading(game, "climb the stairs", Direction.Up),
            Heading(game, "climb down", Direction.Down),
            GlobalCommand(game, "where am I"),
            GlobalCommand(game, "look"),
            GlobalCommand(game, "l"),
            GlobalCommand(game, "inventory"),
            GlobalCommand(game, "i"),
            GlobalCommand(game, "take all"),
            GlobalCommand(game, "get all"),
            GlobalCommand(game, "drop all"),
            GlobalCommand(game, "wait"),
            GlobalCommand(game, "z"),
            GlobalCommand(game, "score"),
            GlobalCommand(game, "diagnose"),
            SystemCommand(game, "save"),
            SystemCommand(game, "restore"),
            SystemCommand(game, "quit"),
            SystemCommand(game, "restart"),

            // ---------------------------------------------------------------- the grammar: verb + noun
            At("WestOfHouse", "open mailbox", Act("open", "mailbox")),
            At("WestOfHouse", "open the mailbox", Act("open", "mailbox")),
            At("WestOfHouse", "close the mailbox", Act("close", "mailbox")),
            At("WestOfHouse", "examine mailbox", Act("examine", "mailbox")),
            At("WestOfHouse", "examine the mailbox", Act("examine", "mailbox")),
            At("WestOfHouse", "x mailbox", Act("examine", "mailbox").TypedAs("x")),
            At("WestOfHouse", "look at the mailbox", Act("examine", "mailbox").OrVerb("look")),
            At("WestOfHouse", "look in the mailbox", Act("examine", "mailbox").OrVerb("look", "search", "open")),
            At("WestOfHouse", "look inside the mailbox", Act("examine", "mailbox").OrVerb("look", "search", "open")),
            At("WestOfHouse", "read leaflet", Act("read", "leaflet")),
            At("WestOfHouse", "read the leaflet", Act("read", "leaflet")),
            At("MachineRoom", "examine machine", Act("examine", "machine")),
            At("LivingRoom", "move the rug", Act("move", "rug")),
            At("LivingRoom", "pull the rug", Act("pull", "rug").OrVerb("move")),
            At("LivingRoom", "rearrange the rug", Act("rearrange", "rug").OrVerb("move")),
            At("LivingRoom", "open the trap door", Act("open", "trap door")),
            At("LivingRoom", "examine the trophy case", Act("examine", "trophy case")),
            At("Kitchen", "eat the lunch", Act("eat", "lunch")),
            At("Kitchen", "open the bottle", Act("open", "bottle")),
            At("Kitchen", "shake the bottle", Act("shake", "bottle")),
            At("Kitchen", "smell the garlic", Act("smell", "garlic")),
            At("EndOfRainbow", "wave the sceptre", Act("wave", "sceptre")),
            At("EntranceToHades", "ring the bell", Act("ring", "bell")),
            At("EntranceToHades", "burn the book", Act("burn", "book")),
            At("Clearing", "count the leaves", Act("count", "leaves")),
            At("Clearing", "search the leaves", Act("search", "leaves")),
            At("ForestPath", "wind the canary", Act("wind", "canary")),
            At("SandyCave", "dig sand", Act("dig", "sand")),

            // ---------------------------------------------------------------- the grammar: take and drop
            At("LivingRoom", "take lamp", Take("lamp")),
            At("LivingRoom", "take the lamp", Take("lamp")),
            At("LivingRoom", "get lamp", Take("lamp").TypedAs("get")),
            At("LivingRoom", "grab the lamp", Take("lamp").TypedAs("grab")),
            At("LivingRoom", "pick up the lamp", Take("lamp")),
            At("LivingRoom", "take the brass lantern", Take("brass lantern").OrNoun("lantern", "lamp")),
            At("WestOfHouse", "take mailbox", Take("mailbox")),
            At("WestOfHouse", "grab the bottle", Take("bottle").TypedAs("grab")),
            At("DamBase", "take plastic", Take("plastic")),
            At("DamBase", "take pile", Take("pile")),
            At("LivingRoom", "drop sword", Drop("sword")),
            At("LivingRoom", "drop the sword", Drop("sword")),

            // ---------------------------------------------------------------- the grammar: multi-word verbs
            At("LivingRoom", "turn on lamp", Act("activate", "lamp")),
            At("LivingRoom", "turn on the lamp", Act("activate", "lamp")),
            At("LivingRoom", "switch on the lamp", Act("activate", "lamp")),
            At("LivingRoom", "turn off lamp", Act("deactivate", "lamp")),
            At("LivingRoom", "turn off the lamp", Act("deactivate", "lamp")),
            At("LivingRoom", "switch off the lamp", Act("deactivate", "lamp")),
            At("LivingRoom", "light the lamp", Act("activate", "lamp").TypedAs("light")),
            At("LivingRoom", "light lantern", Act("activate", "lantern").TypedAs("light")),
            At("LivingRoom", "extinguish the lamp", Act("deactivate", "lamp").TypedAs("extinguish")),

            // ---------------------------------------------------------------- the grammar: verb + preposition + noun
            At("LivingRoom", "look under the rug", Act("look", "rug").WithAdverb("under")),
            At("LivingRoom", "look under rug", Act("look", "rug").WithAdverb("under")),
            At("LivingRoom", "peek under rug", Act("look", "rug").TypedAs("peek").WithAdverb("under")),
            At("BehindHouse", "look through the window", Act("look", "window").WithAdverb("through")),
            At("BehindHouse", "peer through the window", Act("look", "window").TypedAs("peer").WithAdverb("through")),

            // ---------------------------------------------------------------- the grammar: two nouns
            At("LivingRoom", "put sword in trophy case", Act("put", "sword", "in", "trophy case")),
            At("LivingRoom", "put the sword in the trophy case", Act("put", "sword", "in", "trophy case")),
            At("LivingRoom", "put the sword into the trophy case",
                Act("put", "sword", "into", "trophy case").OrPreposition("in")),
            At("LivingRoom", "put the sword inside the trophy case",
                Act("put", "sword", "inside", "trophy case").OrPreposition("in")),
            At("TrollRoom", "kill troll with sword", Act("kill", "troll", "with", "sword")),
            At("TrollRoom", "kill the troll with the sword", Act("kill", "troll", "with", "sword")),
            At("TrollRoom", "attack the troll with the sword", Act("attack", "troll", "with", "sword").OrVerb("kill")),
            At("TrollRoom", "kill the thief with the knife", Act("kill", "thief", "with", "knife")),
            At("TrollRoom", "throw the knife at the troll", Act("throw", "knife", "at", "troll")),
            At("DomeRoom", "tie the rope to the railing", Act("tie", "rope", "to", "railing")),
            At("DomeRoom", "tie rope to railing", Act("tie", "rope", "to", "railing")),
            At("DamBase", "inflate the pile of plastic with the air pump",
                Act("inflate", "pile of plastic", "with", "air pump")),
            At("DamBase", "inflate the boat with the pump", Act("inflate", "boat", "with", "pump")),
            At("Dam", "turn the bolt with the wrench", Act("turn", "bolt", "with", "wrench")),
            At("Dam", "Turn the bolt using the wrench.", Act("turn", "bolt", "using", "wrench").OrPreposition("with")),
            At("CyclopsRoom", "give the lunch to the cyclops", Act("give", "lunch", "to", "cyclops")),
            At("WestOfHouse", "unlock the grating with the key", Act("unlock", "grating", "with", "key")),
            At("SandyCave", "dig sand with shovel", Act("dig", "sand", "with", "shovel")),
            // No Zork I object declares "pour"; it is in the shared thesaurus (Verbs.EmptyVerbs), so the
            // grammar vouches for it on any noun.
            At("Kitchen", "pour the water into the bottle",
                Act("pour", "water", "into", "bottle").OrPreposition("in")),

            // ---------------------------------------------------------------- the grammar: getting in, and going places
            At("DamBase", "enter the boat", Board("boat")),
            At("DamBase", "board the magic boat", Board("magic boat").OrNoun("boat")),
            At("BehindHouse", "go to the kitchen", GoTo("kitchen")),
            At("Kitchen", "walk to the cellar", GoTo("cellar")),
            At("Kitchen", "go into the cellar", GoTo("cellar")),
            At("LivingRoom", "head to the kitchen", GoTo("kitchen")),
            At("Kitchen", "travel to the attic", GoTo("attic")),
            At("WestOfHouse", "walk to the north", Move("north").AlsoByGrammar()),
            At("WestOfHouse", "go to the south", Move("south").AlsoByGrammar()),

            // ---------------------------------------------------------------- the AI: a noun the game does not have
            At("WestOfHouse", "put on the hat", Act("don", "hat").AiOnly()),
            At("WestOfHouse", "wear the hat", Act("don", "hat").OrVerb("wear").AiOnly()),
            At("WestOfHouse", "don the hat", Act("don", "hat").AiOnly()),
            At("WestOfHouse", "dress in the hat", Act("don", "hat").AiOnly()),
            At("WestOfHouse", "take off the hat", Act("doff", "hat").AiOnly()),
            At("WestOfHouse", "remove the hat", Act("doff", "hat").OrVerb("remove").AiOnly()),
            At("WestOfHouse", "doff the hat", Act("doff", "hat").AiOnly()),
            At("WestOfHouse", "drop the card", Drop("card").AiOnly()),
            At("DamBase", "set dial to 4775", Act("set", "dial", "to", "4775").AiOnly()),
            At("DamBase", "turn dial to -1", Act("turn", "dial", "to", "-1").AiOnly()),

            // ---------------------------------------------------------------- the AI: a verb the object does not own
            // "kick" is declared (the mirror, the skeleton), but not by the mailbox, so the grammar does
            // not vouch for it there. "cross" belongs to a helper class no noun is tied to.
            At("WestOfHouse", "kick the mailbox", Act("kick", "mailbox").AiOnly()),
            At("EndOfRainbow", "cross the rainbow", Act("cross", "rainbow").AiOnly()),
            At("LivingRoom", "rearrange the lamp", Act("move", "lamp").OrVerb("rearrange").AiOnly()),

            // ---------------------------------------------------------------- the AI: the particle after the object
            At("LivingRoom", "turn lamp on", Act("activate", "lamp").AiOnly()),
            At("LivingRoom", "turn the lamp on", Act("activate", "lamp").AiOnly()),
            At("LivingRoom", "turn lamp off", Act("deactivate", "lamp").AiOnly()),
            At("LivingRoom", "turn the lamp off", Act("deactivate", "lamp").AiOnly()),

            // ---------------------------------------------------------------- the AI: movement in the player's own words
            At("WestOfHouse", "let's head north", Move("north")),
            At("WestOfHouse", "i am going to go north", Move("north")),
            At("WestOfHouse", "let's saunter north", Move("north")),
            At("WestOfHouse", "crawl northly", Move("north")),
            At("WestOfHouse", "let's mosey on north", Move("north")),
            At("WestOfHouse", "southward ho, let's get out of here", Move("south")),
            At("Cellar", "follow the crawlway", Move("south")),
            At("Cellar", "follow the passageway", Move("north")),
            At("Kitchen", "ascend the staircase", Move("up")),
            At("Kitchen", "I want to head up the stairs", Move("up")),
            At("Attic", "descend down", Move("down")),
            At("Attic", "descend the stairs", Move("down")),
            At("Attic", "let's descend the staircase", Move("down")),
            At("Attic", "ok let's climb back down", Move("down")),
            At("EastOfChasm", "follow the path", Move("east")),
            At("EastOfChasm", "follow the narrow passage", Move("north")),
            At("EastOfChasm", "choose the narrow passage", Move("north")),
            At("EastOfChasm", "continue down the path", Move("east")),
            At("EastOfChasm", "just keep following the path", Move("east").AnyDirection()),
            At("BehindHouse", "use the window to go into the house", Move("in")),
            At("BehindHouse", "enter the house through the window", Move("in")),

            // ---------------------------------------------------------------- the AI: going to a place by name
            At("WestOfHouse", "let's make our way over to the cellar", GoTo("cellar").AiOnly()),
            At("Kitchen", "how about we head to the cellar", GoTo("cellar").AiOnly()),
            At("WestOfHouse", "let's head over to the forest", GoTo("forest").AiOnly()),

            // ---------------------------------------------------------------- the AI: getting in and out
            At("DamBase", "get in the boat", Board("boat").AiOnly()),
            At("DamBase", "get inside the boat", Board("boat").AiOnly()),
            At("DamBase", "sit in the boat", Board("boat").AiOnly()),
            At("DamBase", "hop into the boat", Board("boat").AiOnly()),
            At("DamBase", "let's climb aboard the boat", Board("boat").AiOnly()),
            At("DamBase", "let's go for a ride in the magic boat", Board("magic boat").OrNoun("boat").AiOnly()),
            At("DamBase", "alright, let's hop in the boat and go for a ride", Board("boat").AiOnly()),
            At("DamBase", "leave the boat", Disembark("boat")),
            At("DamBase", "exit the boat", Disembark("boat")),
            At("DamBase", "get out of the boat", Disembark("boat")),
            At("DamBase", "clamber out of the boat", Disembark("boat")),
            At("DamBase", "alright, time to get out of the boat", Disembark("boat")),
            At("DamBase", "stand up and get out of the magic boat", Disembark("magic boat").OrNoun("boat")),

            // ---------------------------------------------------------------- the AI: asking, not commanding
            At("WestOfHouse", "what is in my inventory", Inventory()),
            At("WestOfHouse", "what am I carrying?", Inventory()),
            At("WestOfHouse", "tell me everything I have on me", Inventory()),
            At("WestOfHouse", "tell me all the items I am carrying", Inventory()),
            At("WestOfHouse", "can you tell me what's in my pockets right now?", Inventory()),
            At("WestOfHouse", "hey, remind me what I'm lugging around", Inventory()),
            At("WestOfHouse", "what all am I holding onto at the moment?", Inventory()),
            At("WestOfHouse", "what is this place?", Look()),
            At("WestOfHouse", "look at my surroundings", Look()),
            At("WestOfHouse", "tell me about my location", Look()),
            At("WestOfHouse", "so like, where the heck am I right now?", Look()),
            At("WestOfHouse", "hmm, could you describe where I'm standing?", Look()),
            At("WestOfHouse", "remind me what this place looks like again", Look()),

            // ---------------------------------------------------------------- the AI: polite, wordy, or slangy
            At("WestOfHouse", "hey, what do you say we open the mailbox", Act("open", "mailbox").AiOnly()),
            At("WestOfHouse", "next let's try opening the mailbox", Act("open", "mailbox").AiOnly()),
            At("WestOfHouse", "I reckon I'd like to open that there mailbox", Act("open", "mailbox").AiOnly()),
            At("WestOfHouse", "please kick the mailbox", Act("kick", "mailbox").AiOnly()),
            At("WestOfHouse", "give the mailbox a swift kick", Act("kick", "mailbox").AiOnly()),
            At("WestOfHouse", "I want to hug the mailbox", Act("hug", "mailbox").AiOnly()),
            At("WestOfHouse", "the mailbox, eat it", Act("eat", "mailbox").AiOnly()),
            At("WestOfHouse", "the mailbox, please smoke it", Act("smoke", "mailbox").AiOnly()),
            At("WestOfHouse", "it would be great if I can please lick the mailbox", Act("lick", "mailbox").AiOnly()),
            At("WestOfHouse", "crack open the mailbox", Act("open", "mailbox").AnyVerb().AiOnly()),
            At("WestOfHouse", "pry open the mailbox", Act("open", "mailbox").AnyVerb().AiOnly()),
            At("WestOfHouse", "shut the mailbox please", Act("close", "mailbox").AnyVerb().AiOnly()),
            At("WestOfHouse", "smash the mailbox to bits", Act("smash", "mailbox").AnyVerb().AiOnly()),
            At("WestOfHouse", "peruse the leaflet real quick", Act("read", "leaflet").AnyVerb().AiOnly()),
            At("WestOfHouse", "read the leaflet out loud", Act("read", "leaflet").AiOnly()),
            At("WestOfHouse", "ok so I think I'd like to read the leaflet now please", Act("read", "leaflet").AiOnly()),
            At("LivingRoom", "would you kindly examine the trophy case for me", Act("examine", "trophy case").AiOnly()),
            At("LivingRoom", "give the trophy case a good once-over", Act("examine", "trophy case").AnyVerb().AiOnly()),
            At("LivingRoom", "let's go ahead and turn on the lantern, shall we?", Act("activate", "lantern").AiOnly()),
            At("Kitchen", "I'm famished, let me eat the lunch", Act("eat", "lunch").AiOnly()),
            At("Kitchen", "have a drink from the bottle", Act("drink", "bottle").AnyVerb().AiOnly()),
            At("Kitchen", "sip the water", Act("drink", "water").TypedAs("sip")),
            At("LivingRoom", "hey buddy, could you grab the lamp for me?", Take("lamp").AiOnly()),
            At("LivingRoom", "gimme the lamp", Take("lamp").AiOnly()),
            At("LivingRoom", "pretty please drop the sword", Drop("sword").AiOnly()),

            // ---------------------------------------------------------------- the AI: two nouns, said loosely
            At("LivingRoom", "I want to put the sword in the trophy case", Act("put", "sword", "in", "trophy case").AiOnly()),
            At("LivingRoom", "would you please put the sword in the trophy case, ok?",
                Act("put", "sword", "in", "trophy case").AiOnly()),
            At("LivingRoom", "could you stash the sword inside the trophy case for me",
                Act("put", "sword", "inside", "trophy case").AnyVerb().OrPreposition("in", "into").AiOnly()),
            At("TrollRoom", "um, maybe kill the troll with the sword?", Act("kill", "troll", "with", "sword").AiOnly()),
            At("TrollRoom", "I'd like to attack the troll using my sword",
                Act("attack", "troll", "using", "sword").AnyVerb().OrPreposition("with").AiOnly()),
            At("TrollRoom", "yeet the sword at the troll", Act("throw", "sword", "at", "troll").AnyVerb().AiOnly()),
            At("Dam", "Use the wrench to turn the bolt.", Act("turn", "bolt", "with", "wrench").AiOnly()),
            At("Dam", "With the wrench, turn the bolt", Act("turn", "bolt", "with", "wrench").EitherNounOrder().AiOnly()),
            At("Dam", "let's turn the bolt with the wrench", Act("turn", "bolt", "with", "wrench").AiOnly()),
            At("Dam", "go ahead and loosen the bolt using the wrench",
                Act("loosen", "bolt", "using", "wrench").AnyVerb().OrPreposition("with").AiOnly()),

            // ---------------------------------------------------------------- the AI: more than one command, or none
            At("WestOfHouse", "open the mailbox take the leaflet read it", SeveralCommands()),
            At("WestOfHouse", "take the lamp and then go north", SeveralCommands()),
            At("WestOfHouse", "asdf qwer zxcv", Nothing())
        ];
    }
}
