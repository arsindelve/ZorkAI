using Model.Movement;
using static UnitTests.Parser.Meaning;

namespace UnitTests.Parser;

public static partial class ParserCorpus
{
    private static IEnumerable<ParserCase> Planetfall()
    {
        const string game = "Planetfall";
        ParserCase At(string location, string input, Meaning meaning) => meaning.Build(game, location, input);

        return
        [
            // ---------------------------------------------------------------- answered before any parsing
            Heading(game, "north", Direction.N),
            Heading(game, "fore", Direction.N),
            Heading(game, "aft", Direction.S),
            Heading(game, "port", Direction.W),
            Heading(game, "starboard", Direction.E),
            Heading(game, "up", Direction.Up),
            Heading(game, "walk east", Direction.E),
            GlobalCommand(game, "look"),
            GlobalCommand(game, "i"),
            GlobalCommand(game, "wait"),
            GlobalCommand(game, "take all"),
            GlobalCommand(game, "diagnose"),
            GlobalCommand(game, "sleep"),
            GlobalCommand(game, "what time is it"),
            SystemCommand(game, "save"),
            SystemCommand(game, "restore"),

            // ---------------------------------------------------------------- the grammar: verb + noun
            At("Brig", "examine the door", Act("examine", "door")),
            At("ToolRoom", "examine the pliers", Act("examine", "pliers")),
            At("ToolRoom", "examine the laser", Act("examine", "laser")),
            At("Kitchen", "open the canteen", Act("open", "canteen")),
            At("Kitchen", "close canteen", Act("close", "canteen")),
            At("Kitchen", "drink the liquid", Act("drink", "liquid")),
            At("DeckNine", "open the survival kit", Act("open", "survival kit")),
            At("DeckNine", "empty the kit", Act("empty", "kit")),
            At("DeckNine", "eat the goo", Act("eat", "goo")),
            At("DeckNine", "read the diary", Act("read", "diary")),
            At("AdminCorridor", "examine the rift", Act("examine", "rift")),
            At("AdminCorridor", "extend the ladder", Act("extend", "ladder")),
            At("AdminCorridor", "collapse the ladder", Act("collapse", "ladder").OrVerb("retract", "close")),
            At("AdminCorridorSouth", "examine the metal bar, curved into a u-shape",
                Act("examine", "metal bar, curved into a u-shape").OrNoun("metal bar", "bar", "magnet")),
            At("AlfieControlEast", "pull the lever", Act("pull", "lever")),
            At("AlfieControlEast", "push the lever", Act("push", "lever")),
            At("ReactorElevator", "press the up button", Act("press", "up button")),
            At("ReactorElevator", "push the down button", Act("push", "down button")),
            At("MachineShop", "push the green button", Act("push", "green button")),
            At("ElevatorLobby", "press the red button", Act("press", "red button")),
            At("ElevatorLobby", "press the blue button", Act("press", "blue button")),
            At("ComputerRoom", "examine the terminal", Act("examine", "terminal")),
            At("DormA", "wear the uniform", Act("don", "uniform").TypedAs("wear")),

            // ---------------------------------------------------------------- the grammar: take and drop
            At("ToolRoom", "take the pliers", Take("pliers")),
            At("ToolRoom", "get the laser", Take("laser").TypedAs("get")),
            At("Kitchen", "pick up the canteen", Take("canteen")),
            At("ToolRoom", "take the good bedistor", Take("good bedistor").OrNoun("bedistor")),
            At("MessCorridor", "drop the access card", Drop("access card").OrNoun("card")),
            At("MessCorridor", "drop upper elevator access card",
                Drop("upper elevator access card").OrNoun("upper elevator card", "elevator access card", "card")),
            At("MessCorridor", "drop the kitchen access card", Drop("kitchen access card").OrNoun("card")),

            // ---------------------------------------------------------------- the grammar: multi-word verbs
            At("DormA", "put on the uniform", Act("don", "uniform")),
            At("DormA", "take off the uniform", Act("doff", "uniform")),
            At("ComputerRoom", "turn on the terminal", Act("activate", "terminal")),
            At("ComputerRoom", "turn off the terminal", Act("deactivate", "terminal")),
            At("ComputerRoom", "deactivate the terminal", Act("deactivate", "terminal")),
            At("Kitchen", "look at the canteen", Act("examine", "canteen").OrVerb("look")),
            At("Kitchen", "look inside the canteen", Act("examine", "canteen").OrVerb("look", "search", "open")),

            // ---------------------------------------------------------------- the grammar: verb + preposition + noun
            At("BioLockEast", "look through the window", Act("look", "window").WithAdverb("through")),
            At("BioLockEast", "peer through the window", Act("look", "window").TypedAs("peer").WithAdverb("through")),
            At("RadiationLab", "look through the crack", Act("look", "crack").WithAdverb("through")),
            At("RadiationLab", "peer through the crack", Act("look", "crack").TypedAs("peer").WithAdverb("through")),

            // ---------------------------------------------------------------- the grammar: two nouns
            At("MessCorridor", "slide kitchen access card through slot",
                Act("slide", "kitchen access card", "through", "slot").OrNoun("card", "access card", "kitchen card")),
            At("MessCorridor", "slide the kitchen card through the slot",
                Act("slide", "kitchen card", "through", "slot").OrNoun("card", "kitchen access card")),
            At("ElevatorLobby", "slide upper elevator access card through slot",
                Act("slide", "upper elevator access card", "through", "slot")
                    .OrNoun("card", "upper elevator card", "upper card", "access card")),
            At("ElevatorLobby", "slide lower elevator access card through slot",
                Act("slide", "lower elevator access card", "through", "slot")
                    .OrNoun("card", "lower elevator card", "lower card", "access card")),
            At("ElevatorLobby", "slide upper card through slot",
                Act("slide", "upper card", "through", "slot").OrNoun("card")),
            At("ElevatorLobby", "slide lower card through slot",
                Act("slide", "lower card", "through", "slot").OrNoun("card")),
            At("KalamonteePlatform", "slide shuttle access card through slot",
                Act("slide", "shuttle access card", "through", "slot").OrNoun("card", "shuttle card", "access card")),
            At("KalamonteePlatform", "slide shuttle card through slot",
                Act("slide", "shuttle card", "through", "slot").OrNoun("card")),
            // The model drops the preposition on some runs of this very input (issue #538); the pipeline
            // reads it back out of the player's own words.
            At("BoothTwo", "slide teleportation card through slot",
                Act("slide", "teleportation card", "through", "slot").OrNoun("card")
                    .WhenTheModelSays(new ModelAnswer("act", "slide", ["teleportation card", "slot"]))),
            At("CourseControl", "put the good bedistor in the cube", Act("put", "good bedistor", "in", "cube")),
            // "good" alone names the bedistor (issue #550).
            At("CourseControl", "put good in cube", Act("put", "good", "in", "cube")),
            At("CourseControl", "remove the fused bedistor with the pliers",
                Act("remove", "fused bedistor", "with", "pliers").OrVerb("take")),
            // A two-noun take is an action, not a take - and the model files it under "take" anyway
            // (issue #538). The tool must not be dropped.
            At("CourseControl", "take fused bedistor with pliers",
                Act("take", "fused bedistor", "with", "pliers")
                    .WhenTheModelSays(new ModelAnswer("take", "take", ["fused bedistor", "pliers"], "with"))),
            At("StripNearRelay", "shoot the speck with the laser", Act("shoot", "speck", "with", "laser")),
            At("StripNearRelay", "fire the laser at the speck", Act("fire", "laser", "at", "speck").OrVerb("shoot")),
            At("AdminCorridorSouth", "get the key with the magnet", Act("get", "key", "with", "magnet").OrVerb("take")),
            At("StorageWest", "unlock the padlock with the key", Act("unlock", "padlock", "with", "key")),

            // ---------------------------------------------------------------- the grammar: getting in, and going places
            At("EscapePod", "enter the web", Board("web").OrNoun("webbing", "safety web")),
            At("DormA", "enter the bed", Board("bed")),
            // A door is boarded by the grammar; a model may equally call it a move inward that names the
            // door (issues #551, #580), or - having read that the blue door is the lobby's way north -
            // simply a move north. The engine walks through the door every way.
            At("ElevatorLobby", "enter the blue door",
                Board("blue door").OrFromAi(Move("in", "blue door")).OrFromAi(Move("north"))),
            At("MessCorridor", "go to the mess hall", GoTo("mess hall")),
            At("MessHall", "go to the kitchen", GoTo("kitchen")),
            At("RecCorridor", "walk to the rec area", GoTo("rec area")),
            At("MechCorridor", "go to mech corridor north", GoTo("mech corridor north")),
            At("ProjectCorridor", "go to the office", GoTo("office")),
            // A model files "go to the <ship direction>" as a trip to a place called "aft". The engine
            // walks aft either way: a destination no room is named for, that is itself a direction, is a
            // move (DestinationNavigationEngine).
            At("DeckNine", "go to the aft",
                Move("aft").AlsoByGrammar().OrFromAi(GoTo("aft"))
                    .WhenTheModelSays(new ModelAnswer("goto", "go", ["aft"]))),

            // ---------------------------------------------------------------- the AI: numbers are not nouns
            At("RecArea", "set the dial to 419", Act("set", "dial", "to", "419").AiOnly()),
            // The same command with the preposition left out by the model (issue #538).
            At("RecArea", "set dial to 419",
                Act("set", "dial", "to", "419").AiOnly()
                    .WhenTheModelSays(new ModelAnswer("act", "set", ["dial", "419"]))),
            At("StripNearRelay", "set laser to 1",
                Act("set", "laser", "to", "1").AiOnly()
                    .WhenTheModelSays(new ModelAnswer("act", "set", ["laser", "1"]))),
            At("StripNearRelay", "set the laser to 2", Act("set", "laser", "to", "2").AiOnly()),
            At("BoothTwo", "press 3", Act("press", "3").OrNoun("button 3", "three").AiOnly()),
            // The model tags the verb and no object at all, because a bare numeral is not a noun phrase
            // (issue #538). The digit is recovered from what the player typed.
            At("BoothTwo", "press 1",
                Act("press", "1").OrNoun("button 1", "one").AiOnly()
                    .WhenTheModelSays(new ModelAnswer("act", "press", []))),
            At("ComputerRoom", "type 384", Act("type", "384").AiOnly()),
            At("ComputerRoom", "type 1", Act("type", "1").OrNoun("one").AiOnly()),
            At("ComputerRoom", "type three", Act("type", "three").OrNoun("3").AiOnly()),
            At("ComputerRoom", "key in 1", Act("key", "1").OrVerb("type").OrNoun("one").AiOnly()),
            At("ComputerRoom", "punch in 1", Act("punch", "1").OrVerb("type").OrNoun("one").AiOnly()),
            At("ReactorElevator", "press up", Act("press", "up").OrNoun("up button").AiOnly()),
            At("ReactorElevator", "press down", Act("press", "down").OrNoun("down button").AiOnly()),

            // ---------------------------------------------------------------- the AI: shapes the grammar leaves alone
            At("AdminCorridor", "place the ladder across the rift", Act("place", "ladder", "across", "rift").OrVerb("put").AiOnly()),
            At("ComputerRoom", "turn the terminal on", Act("activate", "terminal").AiOnly()),
            At("Kitchen", "could you please open the canteen", Act("open", "canteen").AiOnly()),
            At("ToolRoom", "I'd like to have a good look at the pliers", Act("examine", "pliers").OrVerb("look").AiOnly()),
            At("ToolRoom", "grab those pliers for me", Take("pliers").AiOnly()),
            At("AlfieControlEast", "give the lever a pull", Act("pull", "lever").AiOnly()),
            // "yank" and "fetch" are verbs one room declares, for fishing out the key. Aimed at anything
            // else they are answered by nothing, so they must reach the AI, which knows a pull and a take.
            At("AlfieControlEast", "yank the lever", Act("pull", "lever").OrVerb("yank").AiOnly()),
            At("Kitchen", "fetch the canteen", Take("canteen").AiOnly()),

            // ---------------------------------------------------------------- the AI: getting out, and doors
            At("EscapePod", "exit the web", Disembark("web").OrNoun("webbing", "safety web")),
            At("EscapePod", "leave the web", Disembark("web").OrNoun("webbing", "safety web")),
            At("EscapePod", "get out of the safety web", Disembark("safety web").OrNoun("web", "webbing")),
            // "enter door" with the noun dropped and the command filed as a move (issue #580): the noun is
            // read back out of the player's sentence so the engine still knows which door.
            At("ElevatorLobby", "enter door",
                Board("door").OrFromAi(Move("in", "door"))
                    .WhenTheModelSays(new ModelAnswer("move", "enter", [], Direction: "enter"))),
            At("ElevatorLobby", "exit the blue door",
                Disembark("blue door").OrFromAi(Move("out", "blue door")).OrFromAi(Move("north"))),

            // ---------------------------------------------------------------- the AI: asking, not commanding
            At("Brig", "what am I carrying?", Inventory()),
            At("Brig", "what is this place?", Look()),
            // Nonsense. A model may say so, or may dutifully report a verb and a noun; the game has no
            // fleeb either way, and the narrator says as much.
            At("Brig", "blorp the fleeb", Nothing().OrFromAi(Act("blorp", "fleeb")))
        ];
    }

    private static IEnumerable<ParserCase> Stationfall()
    {
        const string game = "Stationfall";
        ParserCase At(string location, string input, Meaning meaning) => meaning.Build(game, location, input);

        return
        [
            // ---------------------------------------------------------------- answered before any parsing
            Heading(game, "fore", Direction.N),
            Heading(game, "down", Direction.Down),
            GlobalCommand(game, "look"),
            GlobalCommand(game, "inventory"),
            SystemCommand(game, "quit"),

            // ---------------------------------------------------------------- the grammar: verb + noun
            At("DeckTwelve", "examine the ceiling", Act("examine", "ceiling")),
            At("DeckTwelve", "examine the wall", Act("examine", "wall")),
            At("Spacetruck", "open the thermos", Act("open", "thermos")),
            At("Spacetruck", "pour the soup", Act("pour", "soup")),
            At("Spacetruck", "taste the soup", Act("taste", "soup")),
            At("Spacetruck", "push the red button", Act("push", "red button")),
            At("Spacetruck", "press the red button", Act("press", "red button")),
            At("CargoBay", "count the pallets", Act("count", "pallets")),
            At("CargoBay", "open the hatch", Act("open", "hatch")),
            At("CargoBay", "search the truck", Act("search", "truck")),
            At("DockingBayTwo", "open the port door", Act("open", "port door").OrNoun("door")),
            At("FormStorageRoom", "crumple the form", Act("crumple", "form")),
            At("RobotPool", "requisition rex", Act("requisition", "rex")),
            At("RobotPool", "choose helen", Act("choose", "helen")),
            At("RobotPool", "hug rex", Act("hug", "rex")),
            At("DeckTwelve", "wear the patrol uniform", Act("don", "patrol uniform").TypedAs("wear").OrNoun("uniform")),

            // ---------------------------------------------------------------- the grammar: take, drop, multi-word verbs
            At("FormStorageRoom", "take the form", Take("form")),
            At("Spacetruck", "drop the thermos", Drop("thermos")),
            At("Spacetruck", "turn on the radio", Act("activate", "radio")),
            At("Spacetruck", "turn off the radio", Act("deactivate", "radio")),

            // ---------------------------------------------------------------- the grammar: an object's own phrase
            // A two-word verb a game class answers is read as typed on that class's nouns: the thermos,
            // the radio, the door and the soup each declare the phrase their handler matches. On any other
            // noun the same phrase is canonicalised or left to the AI, as before.
            At("Spacetruck", "look in the thermos",
                Act("search", "thermos").TypedAs("look in").OrVerb("look", "examine", "open")),
            At("CargoBay", "get in the truck", Board("truck").OrNoun("spacetruck")),
            At("Spacetruck", "listen to the radio", Act("listen", "radio").TypedAs("listen to").OrVerb("hear")),
            At("DockingBayTwo", "knock on the door", Act("knock", "door").TypedAs("knock on").OrNoun("port door")),
            At("Spacetruck", "pour out the soup", Act("pour", "soup").TypedAs("pour out").OrVerb("empty", "dump")),

            // ---------------------------------------------------------------- the grammar: two nouns, and getting in
            At("Spacetruck", "put the soup in the thermos", Act("put", "soup", "in", "thermos")),
            At("CargoBay", "enter the spacetruck", Board("spacetruck").OrNoun("truck")),
            At("CargoBayEntrance", "go to the cargo bay", GoTo("cargo bay")),

            // ---------------------------------------------------------------- the AI: phrases the grammar leaves alone
            // The goo declares "put down", but to everything else it is a drop - which is why the grammar
            // takes a multi-word phrase as typed only on the nouns of the class that answers it.
            At("Spacetruck", "put down the thermos", Drop("thermos").AiOnly()),
            At("Spacetruck", "switch the radio on", Act("activate", "radio").AiOnly())
        ];
    }
}
