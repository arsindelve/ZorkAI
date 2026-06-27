namespace ZorkOne.Tests.Walkthrough;

/// <summary>
/// A "side by side" twin of <see cref="WalkthroughTestOne"/> (the full 350-point win) that travels by
/// NAMING ROOMS — and, wherever possible, by their colloquial SYNONYMS ("go to the galley", "go to the
/// parlor", "go to the basement", "go to the vault") — instead of cardinal directions. The point is to
/// exercise issue #268 destination navigation across a real playthrough, proving the synonym sweep and
/// the resolver hold up end to end.
///
/// Cardinal directions remain only where naming genuinely cannot work: the indistinguishable repeated
/// rooms (the maze, the coal-mine maze, the four "Forest" rooms), the river/boat run, and special verbs
/// (pray, launch, echo, cross the rainbow). Every other step names its destination.
/// </summary>
[TestFixture]
public sealed class WalkthroughByLocationName : WalkthroughTestBase
{
    [Test]
    [TestCase("open mailbox", null, "Opening the small mailbox reveals a leaflet.")]
    [TestCase(
        "read leaflet",
        null,
        "ZORK is a game of adventure, danger, and low cunning. In it you will explore"
    )]
    [TestCase("drop leaflet", null, "Dropped")]
    [TestCase("go to south of house", null, "South of House")]
    [TestCase("go to the backyard", null, "Behind House")]
    [TestCase(
        "open window",
        null,
        "With great effort, you open the window far enough to allow entry."
    )]
    [TestCase("go to the galley", null, "Kitchen", "A quantity of water", "smelling of hot peppers.")]
    [TestCase(
        "go to the parlor",
        null,
        "Living Room",
        "A battery-powered brass lantern",
        "and a large oriental rug in the center of"
    )]
    [TestCase("score", null, "10")]
    [TestCase("take lamp", null, "Taken")]
    [TestCase("move rug", null, "With a great effort,")]
    [TestCase("open trap door", null, "The door reluctantly opens to reveal a rickety staircase ")]
    [TestCase("turn on lantern", null, "The brass lantern is now on.")]
    [TestCase("go to the basement", null, "The trap door crashes shut", "You are in a dark and damp cellar ")]
    [TestCase("score", null, "35")]
    [TestCase("go to east of chasm", null, "You are on the east edge of a chasm")]
    [TestCase("go to the art gallery", null, "Most of the paintings have been stolen by", "Fortunately, there is")]
    [TestCase("take painting", null, "Taken")]
    [TestCase(
        "go to the art studio",
        null,
        "This appears to have been an artist's studio.",
        "Loosely attached to a wall is a small"
    )]
    [TestCase("score", null, "39")]
    [TestCase("go to the galley", null, "Kitchen")]
    [TestCase(
        "go to the loft",
        null,
        "Attic",
        "a nasty-looking knife",
        "A large coil of rope is lying in the corner"
    )]
    [TestCase("take knife", null, "Taken")]
    [TestCase("take rope", null, "Taken")]
    [TestCase("go to the galley", null, "Kitchen")]
    [TestCase(
        "go to the lounge",
        null,
        "Above the trophy case hangs an elvish sword of great antiquity.",
        "Living Room"
    )]
    [TestCase("open case", null, "Opened")]
    [TestCase("put painting in case", null, "Done")]
    [TestCase("score", null, "45")]
    [TestCase("drop knife", null, "Dropped")]
    [TestCase("take sword", null, "Taken")]
    [TestCase("open trap door", null, "The door reluctantly opens")]
    [TestCase("go to the basement", null, "faint blue glow")]
    [TestCase("go to the troll room", null, "Bloodstains")]
    [TestCase("kill the troll with the sword", null, "black fog envelops him")]
    [TestCase("drop sword", null, "Dropped")]
    [TestCase("go to east-west passage", null, "This is a narrow east-west passageway")]
    [TestCase(
        "go to the round room",
        null,
        "This is a circular stone room with passages in all direction",
        "Round Room"
    )]
    [TestCase("score", null, "50")]
    [TestCase("go to engravings cave", null, "There are old engravings on the walls here", "Engravings Cave")]
    [TestCase(
        "go to the dome room",
        null,
        "periphery of a large dome, which forms the ceiling of another room below",
        "Dome Room"
    )]
    [TestCase(
        "tie rope to railing",
        null,
        "The rope drops over the side and comes within ten feet of the floor."
    )]
    [TestCase(
        "go to the torch room",
        null,
        "Torch Room",
        "Sitting on the pedestal is a flaming torch, made of ivory."
    )]
    [TestCase("go to the temple", null, "Temple")]
    [TestCase(
        "go to the tomb",
        null,
        "Egyptian Room",
        "The solid-gold coffin used for the burial of Ramses II is here."
    )]
    [TestCase("take coffin", null, "Taken")]
    [TestCase("score", null, "60")]
    [TestCase("go to the shrine", null, "Temple")]
    [TestCase("go to the altar", null, "Altar")]
    [TestCase("pray", null, "Forest", "sunlight")]
    [TestCase("turn off lantern", null, "The brass lantern is now off")]
    [TestCase("S", null, "This is a dimly lit forest")]
    [TestCase("N", null, "Clearing")]
    [TestCase("go to canyon view", null, "Canyon View", "stretching for miles around")]
    [TestCase("go to the rocky ledge", null, "Rocky Ledge", "which appears climbable")]
    [TestCase("go to the canyon bottom", null, "Canyon Bottom", "Aragain Falls")]
    [TestCase(
        "go to end of rainbow",
        null,
        "End of Rainbow",
        "The beach is narrow due to the presence of the White Cliffs."
    )]
    [TestCase("drop coffin", null, "Dropped")]
    [TestCase("open coffin", null, "The gold coffin opens", "sceptre is ornamented with ")]
    [TestCase("take sceptre", null, "Taken")]
    [TestCase("score", null, "64")]
    [TestCase(
        "wave sceptre",
        null,
        "Suddenly, the rainbow appears to become solid",
        "A shimmering pot of gold"
    )]
    [TestCase("take gold", null, "Taken")]
    [TestCase("score", null, "74")]
    [TestCase("take coffin", null, "Taken")]
    [TestCase("score", null, "74")]
    [TestCase("go to the canyon bottom", null, "Canyon Bottom")]
    [TestCase("go to the rocky ledge", null, "Rocky Ledge")]
    [TestCase("go to canyon view", null, "Canyon View")]
    [TestCase("go to the clearing", null, "Clearing")]
    [TestCase("go to the backyard", null, "Behind House")]
    [TestCase("go to the galley", null, "Kitchen", "A bottle is sitting on the table", "elongated")]
    [TestCase("open sack", null, "Opening the brown sack reveals a lunch, and a clove of garlic")]
    [TestCase("take garlic", null, "Taken")]
    [TestCase("go to the parlor", null, "Living Room", "Your collection of treasures consists of", "A painting")]
    [TestCase("put coffin in case", null, "Done")]
    [TestCase("score", null, "89")]
    [TestCase("put sceptre in case", null, "Done")]
    [TestCase("score", null, "95")]
    [TestCase("put gold in case", null, "Done")]
    [TestCase("score", null, "105")]
    [TestCase("turn on lantern", null, "The brass lantern is now on.")]
    [TestCase("go to the basement", null, "Cellar")]
    [TestCase("go to the troll room", null, "Bloodstains", "Troll Room")]
    [TestCase("go to east-west passage", null, "This is a narrow east-west passageway")]
    [TestCase("go to the abyss", null, "Chasm", "A chasm runs southwest to northeast and the path follows it")]
    [TestCase("go to reservoir south", null, "Reservoir South", "far too deep and wide for crossing")]
    [TestCase("go to the flood control dam", null, "Dam", "You are standing on the top of the Flood Control Dam #3")]
    [TestCase("go to the lobby", null, "Lobby", "Some guidebooks", "Visit Beautiful")]
    [TestCase("take matches", null, "Taken")]
    [TestCase("go to the control room", null, "Maintenance", "ransacked", "blue", "yellow")]
    [TestCase("take wrench", null, "Taken")]
    [TestCase("take screwdriver", null, "Taken")]
    [TestCase("press the yellow button", null, "Click")]
    [TestCase("go to the waiting room", null, "Dam Lobby")]
    [TestCase("go to the dam", null, "Dam", "glowing serenely")]
    [TestCase(
        "turn bolt with wrench",
        null,
        "The sluice gates open and water pours through the dam."
    )]
    [TestCase("drop wrench", null, "Dropped")]
    [TestCase("go to the ravine", null, "Deep Canyon")]
    [TestCase("go to the loud room", null, "It is unbearably loud here, with an ear-splitting")]
    [TestCase("SE", "GoToRoundRoom", "There are old engravings on the walls here")]
    [TestCase("go to the dome room", null, "Dome Room")]
    [TestCase("go to the torch room", null, "Torch Room", "flaming torch, made of ivory.")]
    [TestCase("take torch", null, "Taken")]
    [TestCase("turn off lantern", null, "The brass lantern is now off.")]
    [TestCase("go to the temple", null, "Temple", "There is a brass bell here.")]
    [TestCase("take bell", null, "Taken")]
    [TestCase("go to the shrine", null, "Altar", "altar are burning candles.", "large black book")]
    [TestCase("take book", null, "Taken")]
    [TestCase("take candles", null, "Taken")]
    [TestCase("go to the cave", null, "Cave", "dark, forbidding staircase leading down")]
    [TestCase("go to hell", null, "Entrance to Hades", "who jeer at your attempts to pass.")]
    [TestCase("ring bell", null, "red hot", "as if paralyzed", "candles drop to the ground")]
    [TestCase("take candles", null, "Taken")]
    [TestCase("light a match", null, "One of the matches starts to burn")]
    [TestCase(
        "light candles with match",
        null,
        "candles are lit",
        "The flames flicker"
    )]
    [TestCase("read book", null, "Each word of the prayer reverberates through", "The match has gone")]
    [TestCase("drop book", null, "Dropped")]
    [TestCase(
        "go to the underworld",
        null,
        "You have entered the Land of the Living Dead",
        "It appears to be grinning at you"
    )]
    [TestCase("take skull", null, "Taken")]
    [TestCase("score", null, "129")]
    [TestCase("go to hell", null, "bell", "There is a black book here.")]
    [TestCase("go to the cave", null, "Cave")]
    [TestCase("go to the mirror room", null, "Mirror Room", "You are in a large square room with tall ceilings.")]
    [TestCase(
        "rub mirror",
        null,
        "There is a rumble from deep within the earth and the room shakes"
    )]
    [TestCase("go to the cold passage", null, "Cold Passage", "This is a cold and damp corridor")]
    [TestCase(
        "go to the granite wall",
        null,
        "Slide Room",
        "On the south wall of the chamber the letters \"Granite Wall\""
    )]
    [TestCase(
        "go to the mine entrance",
        null,
        "Mine Entrance",
        "You are standing at the entrance of what might have been a coal mine."
    )]
    [TestCase("go to the squeaky room", null, "Squeaky Room", "You may also escape to the east.")]
    [TestCase(
        "inventory",
        null,
        "garlic",
        "skull",
        "candles",
        "lantern",
        "torch (providing",
        "match",
        "screw"
    )]
    [TestCase("go to the bat room", null, "Bat Room", "holding his nose", "exquisite jade figurine")]
    [TestCase("go to the shaft room", null, "Shaft Room", "the chain is a basket")]
    [TestCase("put torch in basket", null, "Done")]
    [TestCase("put screwdriver in basket", null, "Done")]
    [TestCase("turn on lantern", null, "The brass lantern is now on")]
    [TestCase("go to the smelly room", null, "Smelly Room", "To the south is a narrow tunnel")]
    [TestCase("go to the gas room", null, "Gas Room", "sapphire-encrusted", "short climb up some stairs")]
    [TestCase("E", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("NE", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("SE", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("SW", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("go to the ladder top", null, "Ladder Top", "This is a very small room")]
    [TestCase("go to the ladder bottom", null, "Ladder Bottom", "This is a rather wide room")]
    [TestCase("go to the dead end", null, "dead end in the mine.", "There is a small pile of coal here")]
    [TestCase("take coal", null, "Taken")]
    [TestCase("go to the ladder bottom", null, "Ladder Bottom")]
    [TestCase("go to the ladder top", null, "Ladder Top")]
    [TestCase("Up", null, "Coal Mine")]
    [TestCase("N", null, "Coal Mine")]
    [TestCase("E", null, "Coal Mine")]
    [TestCase("S", null, "Coal Mine")]
    [TestCase("go to the gas room", null, "Gas Room")]
    [TestCase("go to the smelly room", null, "Smelly Room")]
    [TestCase("go to the shaft room", null, "Shaft Room", "A screwdriver", "A torch (p")]
    [TestCase("put coal in basket", null, "Done")]
    [TestCase("lower basket", null, "The basket is lowered to the bottom of the shaft.")]
    [TestCase("go to the smelly room", null, "Smelly Room", "To the south is a narrow tunnel")]
    [TestCase("go to the gas room", null, "Gas Room", "sapphire-encrusted", "short climb up some stairs")]
    [TestCase("E", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("NE", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("SE", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("SW", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("go to the ladder top", null, "Ladder Top", "This is a very small room")]
    [TestCase("go to the ladder bottom", null, "Ladder Bottom", "This is a rather wide room")]
    [TestCase(
        "go to the timber room",
        null,
        "Timber Room",
        "This is a long and narrow passage",
        "a broken timber here"
    )]
    [TestCase("drop all", null, "skull: Dropped", "garlic: Dropped", "lantern: Dropped")]
    [TestCase("go to the drafty room", null, "Drafty Room", "contains", "torch (pr", "coal")]
    [TestCase("score", null, "142")]
    [TestCase("take coal", null, "Taken")]
    [TestCase("take screwdriver", null, "Taken")]
    [TestCase("take torch", null, "Taken")]
    [TestCase("go to the machine room", null, "Machine Room", "large lid, which is closed.")]
    [TestCase("open lid", null, "The lid opens")]
    [TestCase("put coal in machine", null, "Done")]
    [TestCase("close lid", null, "The lid closes")]
    [TestCase("turn switch with screwdriver", null, "display of colored lights and bizarre noises")]
    [TestCase("drop screwdriver", null, "Dropped")]
    [TestCase("open lid", null, "The lid opens revealing a huge diamond")]
    [TestCase("take diamond", null, "Taken")]
    [TestCase("score", null, "152")]
    [TestCase("go to the drafty room", null, "Drafty Room", "At the end of the chain is a basket")]
    [TestCase("put torch in basket", null, "Done")]
    [TestCase("put diamond in basket", null, "Done")]
    [TestCase("go to the timber room", null, "Timber Room", "brass", "clove", "match", "candle", "skull", "broken")]
    [TestCase("take skull", null, "Taken")]
    [TestCase("take lamp", null, "Taken")]
    [TestCase("take garlic", null, "Taken")]
    [TestCase("go to the ladder bottom", null, "Ladder Bottom")]
    [TestCase("go to the ladder top", null, "Ladder Top")]
    [TestCase("Up", null, "Coal Mine")]
    [TestCase("N", null, "Coal Mine")]
    [TestCase("E", null, "Coal Mine")]
    [TestCase("S", null, "Coal Mine")]
    [TestCase("go to the gas room", null, "Gas Room", "bracelet")]
    [TestCase("take bracelet", null, "Taken")]
    [TestCase("score", null, "157")]
    [TestCase("go to the smelly room", null, "Smelly Room")]
    [TestCase("go to the shaft room", null, "Shaft Room")]
    [TestCase("raise basket", null, "The basket is raised to the top of the shaft")]
    [TestCase("examine basket", null, "huge diamond", "torch")]
    [TestCase("take torch", null, "Taken")]
    [TestCase("take diamond", null, "Taken")]
    [TestCase("turn off lantern", null, "now off")]
    [TestCase("go to the bat room", null, "Bat Room", "jade", "deranged")]
    [TestCase("take jade", null, "Taken")]
    [TestCase("score", null, "162")]
    [TestCase("go to the squeaky room", null, "Squeaky Room")]
    [TestCase("go to the mine entrance", null, "Mine Entrance")]
    [TestCase("go to the granite wall", null, "Slide Room")]
    [TestCase("go to the basement", null, "Cellar")]
    [TestCase("go to the parlor", null, "Living", "sceptre", "pot", "coffin", "painting")]
    [TestCase("inventory", null, "jade", "torch", "diamond", "garlic", "lantern")]
    [TestCase("put jade in case", null, "Done")]
    [TestCase("score", null, "167")]
    [TestCase("put diamond in case", null, "Done")]
    [TestCase("score", null, "177")]
    [TestCase("turn on lantern", null, "now on")]
    [TestCase("go to the basement", null, "Cellar")]
    [TestCase("go to the troll room", null, "Troll Room")]
    [TestCase("go to east-west passage", null, "East-West")]
    [TestCase("go to the crevasse", null, "Chasm")]
    [TestCase("go to reservoir south", null, "Reservoir South", "formerly a lake")]
    [TestCase("go to the lake", null, "Reservoir", "used to be a", "mud pile", "bulging with jewels")]
    [TestCase("take trunk", null, "Taken")]
    [TestCase("score", null, "192")]
    [TestCase("go to reservoir north", null, "Reservoir North", "cavernous", "There is a hand-held air pump here")]
    [TestCase("take pump", null, "Taken")]
    [TestCase(
        "go to the atlantis room",
        null,
        "Atlantis Room",
        "long under water",
        "On the shore lies Poseidon's own crystal trident"
    )]
    [TestCase("take trident", null, "Your load is too heavy")]
    [TestCase("inventory", null, "pump", "trunk", "torch", "garlic", "lantern")]
    [TestCase("drop torch", null, "Dropped")]
    [TestCase("take trident", null, "Taken")]
    [TestCase("score", null, "196")]
    [TestCase("go to reservoir north", null, "Reservoir N")]
    [TestCase("go to the lake", null, "Reservoir")]
    [TestCase("go to reservoir south", null, "Reservoir S")]
    [TestCase("go to the dam", null, "Dam", "wrench here")]
    [TestCase(
        "go to the dam base",
        null,
        "Dam Base",
        "river as it winds its way downstream",
        "There is a folded pile of plastic"
    )]
    [TestCase("inflate plastic with pump", null, "The boat inflates and appears seaworthy")]
    [TestCase("drop pump", null, "Dropped")]
    [TestCase("get in the boat", null, "You are now in the magic boat")]
    [TestCase(
        "launch",
        null,
        "Frigid River, in the magic boat",
        "tan label",
        "vicinity of the Dam"
    )]
    [TestCase(
        "wait",
        null,
        "Frigid River, in the magic boat",
        "carries you",
        "The river turns a corner",
        "Time passes"
    )]
    [TestCase(
        "wait",
        null,
        "Frigid River, in the magic boat",
        "descends here into a valley",
        "Time passes",
        "carries you"
    )]
    [TestCase(
        "wait",
        null,
        "Frigid River, in the magic boat",
        "running faster here",
        "Time passes",
        "carries you",
        "sandy beach",
        "red buoy"
    )]
    [TestCase("take buoy", null, "Taken")]
    [TestCase(
        "E",
        null,
        "Sandy Beach, in the magic boat",
        "large sandy beach on the east shore",
        "shovel here"
    )]
    [TestCase("leave boat", null, "You are on your own feet again")]
    [TestCase("take shovel", null, "Your load is too heavy")]
    [TestCase("drop garlic", null, "Dropped")]
    [TestCase("take shovel", null, "Your load is too heavy")]
    [TestCase("drop buoy", null, "Dropped")]
    [TestCase("take shovel", null, "Taken")]
    [TestCase("go to the sandy cave", null, "Sandy Cave", "This is a sand-filled cave")]
    [TestCase("dig in sand with shovel", null, "You seem to be digging a hole here")]
    [TestCase("dig in sand with shovel", null, "The hole is getting deeper, but that's about it")]
    [TestCase("dig in sand with shovel", null, "You are surrounded by a wall of sand on all sides")]
    [TestCase("dig in sand with shovel", null, "You can see a scarab here in the sand")]
    [TestCase("take scarab", null, "Taken")]
    [TestCase("score", null, "201")]
    [TestCase("drop shovel", null, "Dropped")]
    [TestCase("go to the sandy beach", null, "Sandy Beach", "red buoy", "clove", "magic boat")]
    [TestCase("open buoy", null, "Opening the red buoy reveals a large emerald")]
    [TestCase("take emerald", null, "Taken")]
    [TestCase("score", null, "206")]
    [TestCase(
        "go to the shore",
        null,
        "Shore",
        "You are on the east shore of the river. The water here seems somewhat"
    )]
    [TestCase("go to the waterfall", null, "Aragain", "A solid rainbow spans the falls")]
    [TestCase("cross the rainbow", null, "End of Rainbow")]
    [TestCase("turn off lantern", null, "The brass lantern is now off")]
    [TestCase("go to the canyon bottom", null, "Canyon Bottom")]
    [TestCase("go to the rocky ledge", null, "Rocky Ledge")]
    [TestCase("go to canyon view", null, "Canyon View")]
    [TestCase("go to the clearing", null, "Clearing")]
    [TestCase("go to the backyard", null, "Behind House")]
    [TestCase("go to the galley", null, "Kitchen", "water")]
    [TestCase("go to the lounge", null, "Living Room", "gold coffin", "figurine")]
    [TestCase("inventory", null, "emerald", "scarab", "trident")]
    [TestCase("put emerald in case", null, "Done")]
    [TestCase("score", null, "216")]
    [TestCase("put scarab in case", null, "Done")]
    [TestCase("score", null, "221")]
    [TestCase("put trident in case", null, "Done")]
    [TestCase("score", null, "232")]
    [TestCase("put jewels in case", null, "Done")]
    [TestCase("score", null, "237")]
    [TestCase("go to the galley", null, "Kitchen", "water")]
    [TestCase("go to the backyard", null, "Behind House")]
    [TestCase("go to north of house", null, "North of House")]
    [TestCase("go to the forest path", null, "Forest Path")]
    [TestCase("go to the treetop", null, "Up A Tree")]
    [TestCase("take egg", null, "Taken")]
    [TestCase("score", null, "242")]
    [TestCase("go to the forest path", null, "Forest Path")]
    [TestCase("go to north of house", null, "North of House")]
    [TestCase("go to the backyard", null, "Behind House")]
    [TestCase("go to the galley", null, "Kitchen", "water")]
    [TestCase("go to the parlor", null, "Living Room", "scarab", "emerald")]
    [TestCase("turn on lantern", null, "The brass lantern is now on")]
    [TestCase("go to the basement", null, "Cellar")]
    [TestCase("go to the troll room", null, "Troll Room", "sword", "bloody axe")]
    [TestCase("W", null, "Maze", "twisty little passages, all alike")]
    [TestCase("S", null, "Maze", "twisty little passages, all alike")]
    [TestCase("E", null, "Maze", "twisty little passages, all alike")]
    [TestCase("Up", null, "Maze", "skeleton", "lantern", "key", "coins")]
    [TestCase("take coins", null, "Taken")]
    [TestCase("score", null, "252")]
    [TestCase("take key", null, "Taken")]
    [TestCase("SW", null, "Maze", "twisty little passages, all alike")]
    [TestCase("E", null, "Maze", "twisty little passages, all alike")]
    [TestCase("S", null, "Maze", "twisty little passages, all alike")]
    [TestCase("go to the cyclops room", null, "Cyclops Room", "A cyclops, who looks prepared to eat horses")]
    [TestCase("Ulysses", null, "name of his father's deadly nemesis, flees the room by knocking ")]
    [TestCase("go to the vault", null, "Treasure Room")]
    [TestCase("score", null, "277")]
    [TestCase("give the egg to the thief", null, "taken aback by your unexpected generosity")]
    [TestCase("go to the cyclops room", null, "Cyclops Room")]
    [TestCase("go to the strange passage", null, "Strange Passage", "On the east there is an old wooden door, with a")]
    [TestCase("go to the parlor", null, "Living Room", "coffin", "emerald", "diamond")]
    [TestCase("put coins in case", null, "Done")]
    [TestCase("score", null, "282")]
    [TestCase("take knife", null, "Taken")]
    [TestCase("go to the strange passage", null, "Strange Passage", "On the east there is an old wooden door, with a")]
    [TestCase("go to the cyclops room", null, "Cyclops Room")]
    [TestCase("go to the vault", "PutTheTorchHere", "Treasure Room")] // We left it behind earlier. In the walkthrough this is based on, he must have taken it.
    [TestCase("kill thief with knife", null, "black fog envelops him")]
    [TestCase("take torch", null, "Taken")]
    [TestCase("take chalice", null, "Taken")]
    [TestCase("take egg", null, "Taken")]
    [TestCase("take canary", null, "Taken")]
    [TestCase("score", null, "298")]
    [TestCase("go to the cyclops room", null, "Cyclops Room")]
    [TestCase("NW", null, "Maze", "twisty little passages, all alike")]
    [TestCase("S", null, "Maze", "twisty little passages, all alike")]
    [TestCase("W", null, "Maze", "twisty little passages, all alike")]
    [TestCase("Up", null, "Maze", "twisty little passages, all alike")]
    [TestCase("Down", null, "Maze", "You won't be able to get back up to the tunnel")]
    [TestCase(
        "go to the grating room",
        null,
        "Grating Room",
        "Above you is a grating locked with a skull-and-crossbones lock."
    )]
    [TestCase("unlock grate", null, "(with the skeleton key)", "The grate is unlocked")]
    [TestCase(
        "open grate",
        null,
        "The grating opens to reveal trees above you.",
        "A pile of leaves falls onto"
    )]
    [TestCase("go to the clearing", null, "Clearing", "There is an open grating, descending into darkness")]
    [TestCase("go to the forest path", null, "Forest Path")]
    [TestCase("go to the treetop", null, "Up A Tree")]
    [TestCase(
        "wind canary",
        null,
        "The canary chirps, slightly off-key, an aria from a forgotten opera"
    )]
    [TestCase("go to the forest path", null, "There is a beautiful brass bauble here")]
    [TestCase("take bauble", null, "Taken")]
    [TestCase("score", null, "299")]
    [TestCase("drop knife", null, "Dropped")]
    [TestCase("go to north of house", null, "North of House")]
    [TestCase("go to the backyard", null, "Behind House")]
    [TestCase("go to the galley", null, "Kitchen")]
    [TestCase("go to the parlor", null, "Living Room")]
    [TestCase("put bauble in case", null, "Done")]
    [TestCase("score", null, "300")]
    [TestCase("put chalice in case", null, "Done")]
    [TestCase("score", null, "305")]
    [TestCase("put canary in case", null, "Done")]
    [TestCase("score", null, "309")]
    [TestCase("put egg in case", null, "Done")]
    [TestCase("score", null, "314")]
    [TestCase("put bracelet in case", null, "Done")]
    [TestCase("score", null, "319")]
    [TestCase("put skull in case", null, "Done")]
    [TestCase("score", null, "329")]
    [TestCase("go to the basement", null, "Cellar")]
    [TestCase("go to the troll room", null, "Troll Room")]
    [TestCase("go to east-west passage", null, "East-West")]
    [TestCase("go to the round room", null, "Round Room")]
    [TestCase("go to the loud room", null, "Loud Room")]
    [TestCase("echo", null, "The acoustics of the room")]
    [TestCase("take bar", null, "Taken")]
    [TestCase("score", null, "339")]
    [TestCase("go to the round room", null, "Round Room")]
    [TestCase("go to east-west passage", null, "East-West")]
    [TestCase("go to the troll room", null, "Troll Room")]
    [TestCase("go to the basement", null, "Cellar")]
    [TestCase("go to the parlor", null, "Living Room")]
    [TestCase("put bar in case", null, "Done")]
    [TestCase("score", null, "344")]
    [TestCase("put torch in case", null, "Done", "inaudible voice")]
    [TestCase("score", null, "350")]
    [TestCase("take map", null, "Taken")]
    [TestCase("go to the galley", null, "Kitchen")]
    [TestCase("go to the backyard", null, "Behind House")]
    [TestCase("go to south of house", null, "South of House")]
    [TestCase("go to west of house", null, "West Of House")]
    [TestCase("go to the tomb", null, "Stone Barrow")]
    [TestCase("go to the burial mound", null, "Inside the Barrow", "test your skill and bravery", "350", "ZORK III")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }
}
