namespace ZorkOne.Tests.Walkthrough;

[TestFixture]
public sealed class WalkthroughTestThree : WalkthroughTestBase
{
    // Book: A shortcut through Adventureland: Vol II - Infocom.
    [Test]
    [TestCase("N", null, "North of House")]
    [TestCase("N", null, "Path")]
    [TestCase("Up", null, "Up A Tree")]
    [TestCase("take egg", null, "Taken")]
    [TestCase("Down", null, "Path")]
    [TestCase("S", null, "North of House")]
    [TestCase("E", null, "Behind House")]
    [TestCase("open window", null, "allow entry")]
    [TestCase("W", null, "Kitchen")]
    [TestCase("W", null, "Living Room")]
    [TestCase("take all", null, "Taken")]
    [TestCase("move rug", null, "trap door")]
    [TestCase("open trap door", null, "staircase")]
    [TestCase("open case", null, "Opened")]
    [TestCase("turn on lantern", null, "on")]
    [TestCase("Down", null, "crashes shut")]
    [TestCase("S", null, "Chasm")]
    [TestCase("E", null, "Gallery")]
    [TestCase("take painting", null, "Taken")]
    [TestCase("W", null, "Chasm")]
    [TestCase("N", null, "Cellar")]
    [TestCase("N", null, "Troll")]
    [TestCase("kill the troll with the sword", null, "black fog envelops him")]
    [TestCase("drop sword", null, "Dropped")]
    [TestCase("W", null, "Maze")]
    [TestCase("S", null, "Maze")]
    [TestCase("E", null, "Maze")]
    [TestCase("Up", null, "Maze")]
    [TestCase("take bag", null, "Taken")]
    [TestCase("SW", null, "Maze")]
    [TestCase("E", null, "Maze")]
    [TestCase("S", null, "Maze")]
    [TestCase("SE", null, "Cyclops")]
    [TestCase("Ulysses", null, "name of his father's deadly nemesis, flees the room by knocking ")]
    [TestCase("E", null, "Passage")]
    [TestCase("E", null, "Living Room")]
    [TestCase("open trap door", null, "staircase")]
    [TestCase("put painting in case", null, "Done")]
    [TestCase("put bag in case", null, "Done")]
    [TestCase("put egg in case", null, "Done")]
    [TestCase("Down", null, "Cellar")]
    [TestCase("N", null, "Troll Room")]
    [TestCase("E", null, "Passage")]
    [TestCase("E", null, "Round Room")]
    [TestCase("E", null, "Loud Room")]
    [TestCase("echo", null, "acoustics" )]
    [TestCase("take bar", null, "Taken" )]
    [TestCase("W", null, "Round Room")]
    [TestCase("S", null, "Narrow")]
    [TestCase("S", null, "Mirror")]
    [TestCase("rub mirror", null, "rumble")]
    [TestCase("E", null, "Cave")]
    [TestCase("S", null, "Atlantis")]
    [TestCase("take trident", null, "Taken" )]
    [TestCase("S", null, "North")]
    [TestCase("take pump", null, "Taken" )]
    [TestCase("N", null, "Atlantis")]
    [TestCase("Up", null, "Cave")]
    [TestCase("N", null, "Mirror")]
    [TestCase("rub mirror", null, "rumble")]
    [TestCase("N", null, "Narrow")]
    [TestCase("N", null, "Round")]
    [TestCase("N", null, "South")]
    [TestCase("NE", null, "Deep")]
    [TestCase("E", null, "Dam")]
    [TestCase("E", null, "Dam Base")]
    [TestCase("inflate plastic with pump", null, "The boat inflates and appears seaworthy")]
    [TestCase("put bar in boat", null, "Done")]
    [TestCase("put trident in boat", null, "Done")]
    [TestCase("N", null, "Dam")]
    [TestCase("N", null, "Lobby")]
    [TestCase("N", null, "Maintenance")]
    [TestCase("take screwdriver", null, "Taken" )]
    [TestCase("take wrench", null, "Taken" )]
    [TestCase("press the yellow button", null, "Click")]
    [TestCase("S", null, "Dam Lobby")]
    [TestCase("S", null, "Dam", "glowing serenely")]
    [TestCase(
        "turn bolt with wrench",
        null,
        "The sluice gates open and water pours through the dam."
    )]
    [TestCase("drop wrench", null, "Dropped")]
    [TestCase("E", null, "Dam Base")]
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
    [TestCase("drop buoy", null, "Dropped")]
    [TestCase("take shovel", null, "Taken")]
    [TestCase("NE", null, "Sandy Cave", "This is a sand-filled cave")]
    [TestCase("dig in sand with shovel", null, "You seem to be digging a hole here")]
    [TestCase("dig in sand with shovel", null, "The hole is getting deeper, but that's about it")]
    [TestCase("dig in sand with shovel", null, "You are surrounded by a wall of sand on all sides")]
    [TestCase("dig in sand with shovel", null, "You can see a scarab here in the sand")]
    [TestCase("drop shovel", null, "Dropped")]
    [TestCase("take scarab", null, "Taken")]
    [TestCase("SW", null, "Sandy Beach", "red buoy", "magic boat")]
    [TestCase("open buoy", null, "Opening the red buoy reveals a large emerald")]
    [TestCase("take emerald", null, "Taken")]
    [TestCase("put emerald in boat", null, "Done")]
    [TestCase("put scarab in boat", null, "Done")]
    [TestCase("get in the boat", null, "You are now in the magic boat")]
    [TestCase(
        "launch",
        null,
        "Frigid River, in the magic boat"
    )]
    [TestCase("W", null, "White Cliffs Beach")]
    [TestCase("leave boat", null, "You are on your own feet again")]
    [TestCase("deflate boat", null, "The boat deflates.")]
    [TestCase("take boat", null, "Taken")]
    [TestCase("N", null, "Beach")]
    [TestCase("W", null, "Cave")]
    [TestCase("W", null, "Loud")]
    [TestCase("W", null, "Round")]
    [TestCase("W", null, "Passage")]
    [TestCase("W", null, "Troll")]
    [TestCase("S", null, "Cellar")]
    [TestCase("Up", null, "Living")]
    [TestCase("drop boat", null, "Dropped")]
    [TestCase("inflate plastic with pump", null, "The boat inflates and appears seaworthy")]
    [TestCase("drop pump", null, "Dropped")]
    [TestCase("take scarab", null, "Taken")]
    [TestCase("take emerald", null, "Taken")]
    [TestCase("take trident", null, "Taken")]
    [TestCase("take bar", null, "Taken")]
    [TestCase("put scarab in case", null, "Done")]
    [TestCase("put bar in case", null, "Done")]
    [TestCase("put trident in case", null, "Done")]
    [TestCase("put emerald in case", null, "Done")]
    [TestCase("W", null, "Strange")]
    [TestCase("W", null, "Cyclops")]
    [TestCase("Up", null, "Treasure")]
    [TestCase("say temple", null, "Temple")]
    [TestCase("N", null, "Torch")]
    [TestCase("take torch", null, "Taken")]
    [TestCase("S", null, "Temple")]
    [TestCase("E", null, "Egyptian")]
    [TestCase("open coffin", null, "The sceptre is ornamented with")]
    [TestCase("take sceptre", null, "Taken")]
    [TestCase("take coffin", null, "Taken")]
    [TestCase("W", null, "Temple")]
    [TestCase("S", null, "Altar")]
    [TestCase("pray", null, "Forest")]
    [TestCase("E", null, "Forest")]
    [TestCase("S", null, "North")]
    [TestCase("E", null, "Behind")]
    [TestCase("W", null, "Kitchen")]
    [TestCase("W", null, "Living Room")]
    [TestCase("put coffin in case", null, "Done")]
    [TestCase("drop lantern", null, "Dropped")]
    [TestCase("E", null, "Kitchen")]
    [TestCase("E", null, "Behind")]
    [TestCase("E", null, "Clearing")]
    [TestCase("E", null, "Canyon")]
    [TestCase("Down", null, "Rocky")]
    [TestCase("Down", null, "Bottom")]
    [TestCase("N", null, "Rainbow")]
    [TestCase("wave sceptre", null, "become solid")]
    [TestCase("take pot", null, "Taken")]
    [TestCase("SW", null, "Bottom")]
    [TestCase("Up", null, "Ledge")]
    [TestCase("Up", null, "View")]
    [TestCase("NW", null, "Clearing")]
    [TestCase("W", null, "Behind")]
    [TestCase("W", null, "Kitchen")]
    [TestCase("W", null, "Living")]
    [TestCase("put sceptre in case", null, "Done")]
    [TestCase("put gold in case", null, "Done")]
    [TestCase("E", null, "Kitchen")]
    [TestCase("Up", null, "Attic")]
    [TestCase("take knife", null, "Taken")]
    [TestCase("Down", null, "Kitchen")]
    [TestCase("W", null, "Living")]
    [TestCase("take egg", null, "Taken")]
    [TestCase("W", null, "Strange")]
    [TestCase("W", null, "Cyclops")]
    [TestCase("Up", null, "Treasure")]
    [TestCase("give the egg to the thief", null, "taken aback by your unexpected generosity")]
    [TestCase("kill thief with knife", null, "black fog envelops him")]
    [TestCase("take egg", null, "Taken")]
    [TestCase("drop knife", null, "Dropped")]
    [TestCase("take canary", null, "Taken")]
    [TestCase("take chalice", null, "Taken")]
    [TestCase("Down", null, "Cyclops")]
    [TestCase("E", null, "Strange")]
    [TestCase("E", null, "Living Room")]
    [TestCase("E", null, "Kitchen")]
    [TestCase("E", null, "Behind")]
    [TestCase("E", null, "Clearing")]
    [TestCase("N", null, "Forest")]
    [TestCase(
        "wind canary",
        null,
        "The canary chirps, slightly off-key, an aria from a forgotten opera"
    )]
    [TestCase("take bauble", null, "Taken")]
    [TestCase("S", null, "Clearing")]
    [TestCase("W", null, "Behind")]
    [TestCase("W", null, "Kitchen")]
    [TestCase("W", null, "Living")]
    [TestCase("put canary in case", null, "Done")]
    [TestCase("put egg in case", null, "Done")]
    [TestCase("put bauble in case", null, "Done")]
    [TestCase("put chalice in case", null, "Done")]
    [TestCase("E", null, "Kitchen")]
    [TestCase("take sack", null, "Taken")]
    [TestCase("open sack", null, "Opening the brown sack reveals a lunch, and a clove of garlic")]
    [TestCase("take garlic", null, "Taken")]
    [TestCase("drop sack", null, "Dropped")]
    
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }
}