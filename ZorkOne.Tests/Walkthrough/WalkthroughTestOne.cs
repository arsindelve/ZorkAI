﻿using NUnit.Framework;

namespace ZorkOne.Tests.Walkthrough;

[TestFixture]
public sealed class WalkthroughTestOne : WalkthroughTestBase
{
    // https://web.mit.edu/marleigh/www/portfolio/Files/zork/transcript.html
    [Test]
    [TestCase("open mailbox", null, "Opening the small mailbox reveals a leaflet.")]
    [TestCase(
        "read leaflet",
        null,
        "ZORK is a game of adventure, danger, and low cunning. In it you will explore"
    )]
    [TestCase("drop leaflet", null, "Dropped")]
    [TestCase("S", null, "South of House")]
    [TestCase("E", null, "Behind House")]
    [TestCase(
        "open window",
        null,
        "With great effort, you open the window far enough to allow entry."
    )]
    [TestCase("In", null, "Kitchen", "A quantity of water", "smelling of hot peppers.")]
    [TestCase(
        "W",
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
    [TestCase("Down", null, "The trap door crashes shut", "You are in a dark and damp cellar ")]
    [TestCase("score", null, "35")]
    [TestCase("S", null, "You are on the east edge of a chasm")]
    [TestCase("E", null, "Most of the paintings have been stolen by", "Fortunately, there is")]
    [TestCase("take painting", null, "Taken")]
    [TestCase(
        "N",
        null,
        "This appears to have been an artist's studio.",
        "Loosely attached to a wall is a small"
    )]
    [TestCase("score", null, "39")]
    [TestCase("Up", null, "Kitchen")]
    [TestCase(
        "Up",
        null,
        "Attic",
        "a nasty-looking knife",
        "A large coil of rope is lying in the corner"
    )]
    [TestCase("take knife", null, "Taken")]
    [TestCase("take rope", null, "Taken")]
    [TestCase("Down", null, "Kitchen")]
    [TestCase(
        "W",
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
    [TestCase("Down", null, "faint blue glow")]
    [TestCase("N", "KillTroll", "Bloodstains")]
    [TestCase("drop sword", null, "Dropped")]
    [TestCase("E", null, "This is a narrow east-west passageway")]
    [TestCase(
        "E",
        null,
        "This is a circular stone room with passages in all direction",
        "Round Room"
    )]
    [TestCase("score", null, "50")]
    [TestCase("SE", null, "There are old engravings on the walls here", "Engravings Cave")]
    [TestCase(
        "E",
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
        "Down",
        null,
        "Torch Room",
        "Sitting on the pedestal is a flaming torch, made of ivory."
    )]
    [TestCase("S", null, "Temple")]
    [TestCase(
        "E",
        null,
        "Egyptian Room",
        "The solid-gold coffin used for the burial of Ramses II is here."
    )]
    [TestCase("take coffin", null, "Taken")]
    [TestCase("score", null, "60")]
    [TestCase("W", null, "Temple")]
    [TestCase("S", null, "Altar")]
    [TestCase("pray", null, "Forest", "sunlight")]
    [TestCase("turn off lantern", null, "The brass lantern is now off")]
    [TestCase("S", null, "This is a dimly lit forest")]
    [TestCase("N", null, "Clearing")]
    [TestCase("E", null, "Canyon View", "stretching for miles around")]
    [TestCase("Down", null, "Rocky Ledge", "which appears climbable")]
    [TestCase("Down", null, "Canyon Bottom", "Aragain Falls")]
    [TestCase(
        "N",
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
    [TestCase("SW", null, "Canyon Bottom")]
    [TestCase("Up", null, "Rocky Ledge")]
    [TestCase("Up", null, "Canyon View")]
    [TestCase("NW", null, "Clearing")]
    [TestCase("W", null, "Behind House")]
    [TestCase("In", null, "Kitchen", "A bottle is sitting on the table", "elongated")]
    [TestCase("open sack", null, "Opening the brown sack reveals a lunch, and a clove of garlic")]
    [TestCase("take garlic", null, "Taken")]
    [TestCase("W", null, "Living Room", "Your collection of treasures consists of", "A painting")]
    [TestCase("put coffin in case", null, "Done")]
    [TestCase("score", null, "89")]
    [TestCase("put sceptre in case", null, "Done")]
    [TestCase("score", null, "95")]
    [TestCase("put gold in case", null, "Done")]
    [TestCase("score", null, "105")]
    [TestCase("turn on lantern", null, "The brass lantern is now on.")]
    [TestCase("Down", null, "Cellar")]
    [TestCase("N", null, "Bloodstains", "Troll Room")]
    [TestCase("E", null, "This is a narrow east-west passageway")]
    [TestCase("N", null, "Chasm", "A chasm runs southwest to northeast and the path follows it")]
    [TestCase("NE", null, "Reservoir South", "far too deep and wide for crossing")]
    [TestCase("E", null, "Dam", "You are standing on the top of the Flood Control Dam #3")]
    [TestCase("N", null, "Lobby", "Some guidebooks", "Visit Beautiful")]
    [TestCase("take matches", null, "Taken")]
    [TestCase("N", null, "Maintenance", "ransacked", "blue", "yellow")]
    [TestCase("take wrench", null, "Taken")]
    [TestCase("take screwdriver", null, "Taken")]
    [TestCase("press the yellow button", null, "Click")]
    [TestCase("S", null, "Dam Lobby")]
    [TestCase("S", null, "Dam", "glowing serenely")]
    [TestCase(
        "turn bolt with wrench",
        null,
        "The sluice gates open and water pours through the dam."
    )]
    [TestCase("drop wrench", null, "Dropped")]
    [TestCase("S", null, "Deep Canyon")]
    [TestCase("Down", null, "It is unbearably loud here, with an ear-splitting")]
    [TestCase("SE", "GoToRoundRoom", "There are old engravings on the walls here")]
    [TestCase("E", null, "Dome Room")]
    [TestCase("Down", null, "Torch Room", "flaming torch, made of ivory.")]
    [TestCase("take torch", null, "Taken")]
    [TestCase("turn off lantern", null, "The brass lantern is now off.")]
    [TestCase("S", null, "Temple", "There is a brass bell here.")]
    [TestCase("take bell", null, "Taken")]
    [TestCase("S", null, "Altar", "altar are burning candles.", "large black book")]
    [TestCase("take book", null, "Taken")]
    [TestCase("take candles", null, "Taken")]
    [TestCase("Down", null, "Cave", "dark, forbidding staircase leading down")]
    [TestCase("Down", null, "Entrance to Hades", "who jeer at your attempts to pass.")]
    [TestCase("ring bell", null, "red hot", "as if paralyzed", "candles drop to the ground")]
    [TestCase("take candles", null, "Taken")]
    [TestCase("light a match", null, "One of the matches starts to burn")]
    [TestCase(
        "light candles with match",
        null,
        "candles are lit",
        "The match has gone",
        "The flames flicker"
    )]
    [TestCase("read book", null, "Each word of the prayer reverberates through")]
    [TestCase("drop book", null, "Dropped")]
    [TestCase(
        "S",
        null,
        "You have entered the Land of the Living Dead",
        "It appears to be grinning at you"
    )]
    [TestCase("take skull", null, "Taken")]
    [TestCase("score", null, "129")]
    [TestCase("N", null, "bell", "There is a black book here.")]
    [TestCase("Up", null, "Cave")]
    [TestCase("N", null, "Mirror Room", "You are in a large square room with tall ceilings.")]
    [TestCase(
        "rub mirror",
        null,
        "There is a rumble from deep within the earth and the room shakes"
    )]
    [TestCase("N", null, "Cold Passage", "This is a cold and damp corridor")]
    [TestCase(
        "W",
        null,
        "Slide Room",
        "On the south wall of the chamber the letters \"Granite Wall\""
    )]
    [TestCase(
        "N",
        null,
        "Mine Entrance",
        "You are standing at the entrance of what might have been a coal mine."
    )]
    [TestCase("W", null, "Squeaky Room", "You may also escape to the east.")]
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
    [TestCase("N", null, "Bat Room", "holding his nose", "exquisite jade figurine")]
    [TestCase("E", null, "Shaft Room", "the chain is a basket")]
    [TestCase("put torch in basket", null, "Done")]
    [TestCase("put screwdriver in basket", null, "Done")]
    [TestCase("turn on lantern", null, "The brass lantern is now on")]
    [TestCase("N", null, "Smelly Room", "To the south is a narrow tunnel")]
    [TestCase("Down", null, "Gas Room", "sapphire-encrusted", "short climb up some stairs")]
    [TestCase("E", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("NE", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("SE", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("SW", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("Down", null, "Ladder Top", "This is a very small room")]
    [TestCase("Down", null, "Ladder Bottom", "This is a rather wide room")]
    [TestCase("S", null, "dead end in the mine.", "There is a small pile of coal here")]
    [TestCase("take coal", null, "Taken")]
    [TestCase("N", null, "Ladder Bottom")]
    [TestCase("Up", null, "Ladder Top")]
    [TestCase("Up", null, "Coal Mine")]
    [TestCase("N", null, "Coal Mine")]
    [TestCase("E", null, "Coal Mine")]
    [TestCase("S", null, "Coal Mine")]
    [TestCase("N", null, "Gas Room")]
    [TestCase("Up", null, "Smelly Room")]
    [TestCase("S", null, "Shaft Room", "A screwdriver", "A torch (p")]
    [TestCase("put coal in basket", null, "Done")]
    [TestCase("lower basket", null, "The basket is lowered to the bottom of the shaft.")]
    [TestCase("N", null, "Smelly Room", "To the south is a narrow tunnel")]
    [TestCase("Down", null, "Gas Room", "sapphire-encrusted", "short climb up some stairs")]
    [TestCase("E", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("NE", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("SE", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("SW", null, "Coal Mine", "This is a nondescript part of a coal mine")]
    [TestCase("Down", null, "Ladder Top", "This is a very small room")]
    [TestCase("Down", null, "Ladder Bottom", "This is a rather wide room")]
    [TestCase(
        "W",
        null,
        "Timber Room",
        "This is a long and narrow passage",
        "a broken timber here"
    )]
    [TestCase("drop all", null, "skull: Dropped", "garlic: Dropped", "lantern: Dropped")]
    [TestCase("W", null, "Drafty Room", "contains", "torch (pr", "coal")]
    [TestCase("score", null, "142")]
    [TestCase("take coal", null, "Taken")]
    [TestCase("take screwdriver", null, "Taken")]
    [TestCase("take torch", null, "Taken")]
    [TestCase("S", null, "Machine Room", "large lid, which is closed.")]
    [TestCase("open lid", null, "The lid opens")]
    [TestCase("put coal in machine", null, "Done")]
    [TestCase("close lid", null, "The lid closes")]
    [TestCase("turn switch with screwdriver", null, "display of colored lights and bizarre noises")]
    [TestCase("drop screwdriver", null, "Dropped")]
    [TestCase("open lid", null, "The lid opens revealing a huge diamond")]
    [TestCase("take diamond", null, "Taken")]
    [TestCase("score", null, "152")]
    [TestCase("N", null, "Drafty Room", "At the end of the chain is a basket")]
    [TestCase("put torch in basket", null, "Done")]
    [TestCase("put diamond in basket", null, "Done")]
    [TestCase("E", null, "Timber Room", "brass", "clove", "match", "candle", "skull", "broken")]
    [TestCase("take skull", null, "Taken")]
    [TestCase("take lamp", null, "Taken")]
    [TestCase("take garlic", null, "Taken")]
    [TestCase("E", null, "Ladder Bottom")]
    [TestCase("Up", null, "Ladder Top")]
    [TestCase("Up", null, "Coal Mine")]
    [TestCase("N", null, "Coal Mine")]
    [TestCase("E", null, "Coal Mine")]
    [TestCase("S", null, "Coal Mine")]
    [TestCase("N", null, "Gas Room", "bracelet")]
    [TestCase("take bracelet", null, "Taken")]
    [TestCase("score", null, "157")]
    [TestCase("Up", null, "Smelly Room")]
    [TestCase("S", null, "Shaft Room")]
    [TestCase("raise basket", null, "The basket is raised to the top of the shaft")]
    [TestCase("examine basket", null, "huge diamond", "torch")]
    [TestCase("take torch", null, "Taken")]
    [TestCase("take diamond", null, "Taken")]
    [TestCase("turn off lantern", null, "now off")]
    [TestCase("W", null, "Bat Room", "jade", "deranged")]
    [TestCase("take jade", null, "Taken")]
    [TestCase("score", null, "162")]
    [TestCase("S", null, "Squeaky Room")]
    [TestCase("E", null, "Mine Entrance")]
    [TestCase("S", null, "Slide Room")]
    [TestCase("Down", null, "Cellar")]
    [TestCase("Up", null, "Living", "sceptre", "pot", "coffin", "painting")]
    [TestCase("inventory", null, "jade", "torch", "diamond", "garlic", "lantern")]
    [TestCase("put jade in case", null, "Done")]
    [TestCase("score", null, "167")]
    [TestCase("put diamond in case", null, "Done")]
    [TestCase("score", null, "177")]
    [TestCase("turn on lantern", null, "now on")]
    [TestCase("Down", null, "Cellar")]
    [TestCase("N", null, "Troll Room")]
    [TestCase("E", null, "East-West")]
    [TestCase("N", null, "Chasm")]
    [TestCase("NE", null, "Reservoir South", "formerly a lake")]
    [TestCase("N", null, "Reservoir", "used to be a", "mud pile", "bulging with jewels")]
    [TestCase("take trunk", null, "Taken")]
    [TestCase("score", null, "192")]
    [TestCase("N", null, "Reservoir North", "cavernous", "There is a hand-held air pump here")]
    [TestCase("take pump", null, "Taken")]
    [TestCase(
        "N",
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
    [TestCase("S", null, "Reservoir N")]
    [TestCase("S", null, "Reservoir")]
    [TestCase("S", null, "Reservoir S")]
    [TestCase("E", null, "Dam", "wrench here")]
    [TestCase(
        "E",
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
    [TestCase("NE", null, "Sandy Cave", "This is a sand-filled cave")]
    [TestCase("dig in sand with shovel", null, "You seem to be digging a hole here")]
    [TestCase("dig in sand with shovel", null, "The hole is getting deeper, but that's about it")]
    [TestCase("dig in sand with shovel", null, "You are surrounded by a wall of sand on all sides")]
    [TestCase("dig in sand with shovel", null, "You can see a scarab here in the sand")]
    [TestCase("take scarab", null, "Taken")]
    [TestCase("score", null, "201")]
    [TestCase("drop shovel", null, "Dropped")]
    [TestCase("SW", null, "Sandy Beach", "red buoy", "clove", "magic boat")]
    [TestCase("open buoy", null, "Opening the red buoy reveals a large emerald")]
    [TestCase("take emerald", null, "Taken")]
    [TestCase("score", null, "206")]
    [TestCase(
        "S",
        null,
        "Shore",
        "You are on the east shore of the river. The water here seems somewhat"
    )]
    [TestCase("S", null, "Aragain", "A solid rainbow spans the falls")]
    [TestCase("cross the rainbow", null, "End of Rainbow")]
    [TestCase("turn off lantern", null, "The brass lantern is now off")]
    [TestCase("SW", null, "Canyon Bottom")]
    [TestCase("Up", null, "Rocky Ledge")]
    [TestCase("Up", null, "Canyon View")]
    [TestCase("NW", null, "Clearing")]
    [TestCase("W", null, "Behind House")]
    [TestCase("W", null, "Kitchen", "water")]
    [TestCase("W", null, "Living Room", "gold coffin", "figurine")]
    [TestCase("inventory", null, "emerald", "scarab", "trident")]
    [TestCase("put emerald in case", null, "Done")]
    [TestCase("score", null, "216")]
    [TestCase("put scarab in case", null, "Done")]
    [TestCase("score", null, "221")]
    [TestCase("put trident in case", null, "Done")]
    [TestCase("score", null, "232")]
    [TestCase("put jewels in case", null, "Done")]
    [TestCase("score", null, "237")]
    [TestCase("E", null, "Kitchen", "water")]
    [TestCase("E", null, "Behind House")]
    [TestCase("N", null, "North of House")]
    [TestCase("N", null, "Forest Path")]
    [TestCase("Up", null, "Up A Tree")]
    [TestCase("take egg", null, "Taken")]
    [TestCase("score", null, "242")]
    [TestCase("Down", null, "Forest Path")]
    [TestCase("S", null, "North of House")]
    [TestCase("E", null, "Behind House")]
    [TestCase("W", null, "Kitchen", "water")]
    [TestCase("W", null, "Living Room", "scarab", "emerald")]
    [TestCase("turn on lantern", null, "The brass lantern is now on")]
    [TestCase("Down", null, "Cellar")]
    [TestCase("N", null, "Troll Room", "sword", "bloody axe")]
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
    [TestCase("SE", null, "Cyclops Room", "A cyclops, who looks prepared to eat horses")]
    [TestCase("Ulysses", null, "name of his father's deadly nemesis, flees the room by knocking ")]
    [TestCase("Up", null, "Treasure Room")]
    [TestCase("score", null, "277")]
    // TODO: give egg to thief
    [TestCase("Down", null, "Cyclops Room")]
    [TestCase("E", null, "Strange Passage", "On the east there is an old wooden door, with a")]
    [TestCase("E", null, "Living Room", "coffin", "emerald", "diamond")]
    [TestCase("put coins in case", null, "Done")]
    [TestCase("score", null, "282")]
    [TestCase("take knife", null, "Taken")]
    [TestCase("W", null, "Strange Passage", "On the east there is an old wooden door, with a")]
    [TestCase("W", null, "Cyclops Room")]
    // TODO: kill the thief and take his stuff
    [TestCase("Up", "PutTheTorchHere", "Treasure Room")] // We left it behind earlier. He must have taken it.
    [TestCase("take chalice", null, "Taken")]
    [TestCase("take torch", null, "Taken")]
    [TestCase("score", null, "292")]
    [TestCase("Down", null, "Cyclops Room")]
    [TestCase("NW", null, "Maze", "twisty little passages, all alike")]
    [TestCase("S", null, "Maze", "twisty little passages, all alike")]
    [TestCase("W", null, "Maze", "twisty little passages, all alike")]
    [TestCase("Up", null, "Maze", "twisty little passages, all alike")]
    [TestCase("Down", null, "Maze", "You won't be able to get back up to the tunnel")]
    [TestCase(
        "NE",
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
    [TestCase("Up", null, "Clearing", "There is an open grating, descending into darkness")]
    [TestCase("S", null, "Forest Path")]
    [TestCase("Up", null, "Up A Tree")]
    [TestCase(
        "wind canary",
        "HaveOpenEgg",
        "The canary chirps, slightly off-key, an aria from a forgotten opera"
    )]
    [TestCase("Down", null, "There is a beautiful brass bauble here")]
    [TestCase("take bauble", null, "Taken")]
    [TestCase("score", null, "293")]
    [TestCase("drop knife", null, "Dropped")]
    [TestCase("S", null, "North of House")]
    [TestCase("E", null, "Behind House")]
    [TestCase("W", null, "Kitchen")]
    [TestCase("W", null, "Living Room")]
    [TestCase("put bauble in case", null, "Done")]
    [TestCase("score", null, "294")]
    [TestCase("put chalice in case", null, "Done")]
    [TestCase("score", null, "299")]
    [TestCase("take canary", null, "Taken")]
    [TestCase("score", null, "305")]
    [TestCase("put canary in case", null, "Done")]
    [TestCase("score", null, "309")]
    [TestCase("put egg in case", null, "Done")]
    [TestCase("score", null, "314")]
    [TestCase("put bracelet in case", null, "Done")]
    [TestCase("score", null, "319")]
    [TestCase("put skull in case", null, "Done")]
    [TestCase("score", null, "329")]
    [TestCase("Down", null, "Cellar")]
    [TestCase("N", null, "Troll Room")]
    [TestCase("E", null, "East-West")]
    [TestCase("E", null, "Round Room")]
    [TestCase("E", null, "Loud Room")]
    [TestCase("echo", null, "The acoustics of the room")]
    [TestCase("take bar", null, "Taken")]
    [TestCase("score", null, "339")]
    [TestCase("W", null, "Round Room")]
    [TestCase("W", null, "East-West")]
    [TestCase("W", null, "Troll Room")]
    [TestCase("S", null, "Cellar")]
    [TestCase("Up", null, "Living Room")]
    [TestCase("put bar in case", null, "Done")]
    [TestCase("score", null, "344")]
    [TestCase("put torch in case", null, "Done")]
    [TestCase("score", null, "350")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }
}
