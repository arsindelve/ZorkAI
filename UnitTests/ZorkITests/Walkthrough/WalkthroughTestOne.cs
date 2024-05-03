using ZorkOne;

namespace UnitTests.ZorkITests.Walkthrough;

[TestFixture]
public sealed class WalkthroughTestOne : EngineTestsBase
{
    [OneTimeSetUp]
    public void Init()
    {
        _target = GetTarget();
        Repository.Reset();
    }

    private GameEngine<ZorkI, ZorkIContext> _target;

    [Test]
    [TestCase("open mailbox", null, "Opening the small mailbox reveals a leaflet.")]
    [TestCase("read leaflet", null, "ZORK is a game of adventure, danger, and low cunning. In it you will explore")]
    [TestCase("drop leaflet", null, "Dropped")]
    [TestCase("S", null, "South of House")]
    [TestCase("E", null, "Behind House")]
    [TestCase("open window", null, "With great effort, you open the window far enough to allow entry.")]
    [TestCase("In", null, "Kitchen", "A quantity of water", "smelling of hot peppers.")]
    [TestCase("W", null, "Living Room", "A battery-powered brass lantern", "and a large oriental rug in the center of")]
    [TestCase("take lamp", null, "Taken")]
    [TestCase("move rug", null, "With a great effort,")]
    [TestCase("open trap door", null, "The door reluctantly opens to reveal a rickety staircase ")]
    [TestCase("turn on lantern", null, "The brass lantern is now on.")]
    [TestCase("go down", null, "The trap door crashes shut", "You are in a dark and damp cellar ")]
    [TestCase("S", null, "You are on the east edge of a chasm")]
    [TestCase("E", null, "Most of the paintings have been stolen by", "Fortunately, there is")]
    [TestCase("take painting", null, "Taken")]
    [TestCase("N", null, "This appears to have been an artist's studio.", "Loosely attached to a wall is a small")]
    [TestCase("Up", null, "Kitchen")]
    [TestCase("Up", null, "Attic", "a nasty-looking knife", "A large coil of rope is lying in the corner")]
    [TestCase("take knife", null, "Taken")]
    [TestCase("take rope", null, "Taken")]
    [TestCase("go down", null, "Kitchen")]
    [TestCase("W", null, "Above the trophy case hangs an elvish sword of great antiquity.", "Living Room")]
    [TestCase("open case", null, "Opened")]
    [TestCase("put painting inside case", null, "Done")]
    [TestCase("drop knife", null, "Dropped")]
    [TestCase("take sword", null, "Taken")]
    [TestCase("open trap door", null, "The door reluctantly opens")]
    [TestCase("go down", null, "The trap door crashes shut", "faint blue glow")]
    [TestCase("N", "KillTroll", "Bloodstains")]
    [TestCase("drop sword", null, "Dropped")]
    [TestCase("E", null, "This is a narrow east-west passageway")]
    [TestCase("E", null, "This is a circular stone room with passages in all direction", "Round Room")]
    [TestCase("SE", null, "There are old engravings on the walls here", "Engravings Cave")]
    [TestCase("E", null, "periphery of a large dome, which forms the ceiling of another room below", "Dome Room")]
    [TestCase("tie rope to railing", null, "The rope drops over the side and comes within ten feet of the floor.")]
    [TestCase("go down", null, "Torch Room", "Sitting on the pedestal is a flaming torch, made of ivory.")]
    [TestCase("S", null, "Temple")]
    [TestCase("E", null, "Egyptian Room", "The solid-gold coffin used for the burial of Ramses II is here.")]
    [TestCase("take coffin", null, "Taken")]
    [TestCase("W", null, "Temple")]
    [TestCase("S", null, "Altar")]
    [TestCase("pray", null, "Forest", "sunlight")]
    [TestCase("turn off lantern", null, "The brass lantern is now off")]
    [TestCase("S", null, "This is a dimly lit forest")]
    [TestCase("N", null, "Clearing")]
    [TestCase("E", null, "Canyon View", "stretching for miles around")]
    [TestCase("Down", null, "Rocky Ledge", "which appears climbable")]
    [TestCase("Down", null, "Canyon Bottom", "Aragain Falls")]
    [TestCase("N", null, "End of Rainbow", "The beach is narrow due to the presence of the White Cliffs.")]
    [TestCase("drop coffin", null, "Dropped")]
    [TestCase("open coffin", null, "The gold coffin opens", "sceptre is ornamented with ")]
    [TestCase("take sceptre", null, "Taken")]
    [TestCase("wave sceptre", null, "Suddenly, the rainbow appears to become solid", "A shimmering pot of gold")]
    [TestCase("take gold", null, "Taken")]
    [TestCase("take coffin", null, "Taken")]
    [TestCase("SW", null, "Canyon Bottom")]
    [TestCase("Up", null, "Rocky Ledge")]
    [TestCase("Up", null, "Canyon View")]
    [TestCase("NW", null, "Clearing")]
    [TestCase("W", null, "Behind House")]
    [TestCase("In", null, "Kitchen", "A bottle is sitting on the table", "elongated")]
    [TestCase("open sack", null, "Opening the brown sack reveals a lunch, and a clove of garlic")]
    [TestCase("take garlic", null, "Taken")]
    [TestCase("W", null, "Living Room", "Your collection of treasures consists of", "A painting")]
    [TestCase("put coffin inside case", null, "Done")]
    [TestCase("put sceptre inside case", null, "Done")]
    [TestCase("put gold in case", null, "Done")]
    [TestCase("open trap door", null, "The door reluctantly opens to reveal a")]
    [TestCase("turn on lantern", null, "The brass lantern is now on.")]
    [TestCase("go down", null, "The trap door crashes shut", "Cellar")]
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
    [TestCase("turn bolt with wrench", null, "The sluice gates open and water pours through the dam.")]
    [TestCase("drop wrench", null, "Dropped")]
    [TestCase("S", null, "Deep Canyon")]
    [TestCase("go down", null, "It is unbearably loud here, with an ear-splitting")]
    [TestCase("SE", "GoToRoundRoom", "There are old engravings on the walls here")]
    [TestCase("E", null, "Dome Room")]
    [TestCase("go down", null, "Torch Room", "flaming torch, made of ivory.")]
    [TestCase("take torch", null, "Taken")]
    [TestCase("turn off lantern", null, "The brass lantern is now off.")]
    [TestCase("S", null, "Temple", "There is a brass bell here.")]
    [TestCase("take bell", null, "Taken")]
    [TestCase("S", null, "Altar", "altar are burning candles.", "large black book")]
    [TestCase("take book", null, "Taken")]
    [TestCase("take candles", null, "Taken")]
    [TestCase("go down", null, "Cave", "dark, forbidding staircase leading down")]
    [TestCase("go down", null, "Entrance to Hades", "who jeer at your attempts to pass.")]
    [TestCase("ring bell", null, "red hot", "as if paralyzed", "candles drop to the ground")]
    [TestCase("take candles", null, "Taken")]
    [TestCase("light a match", null, "One of the matches starts to burn")]
    [TestCase("light candles with match", null, "candles are lit", "The match has gone", "The flames flicker")]
    [TestCase("read book", null, "Each word of the prayer reverberates through")]
    [TestCase("drop book", null, "Dropped")]
    [TestCase("S", null, "You have entered the Land of the Living Dead", "It appears to be grinning at you")]
    [TestCase("take skull", null, "Taken")]
    [TestCase("N", null, "bell", "There is a black book here.")]
    [TestCase("Up", null, "Cave")]
    [TestCase("N", null, "Mirror Room", "You are in a large square room with tall ceilings.")]
    [TestCase("rub mirror", null, "There is a rumble from deep within the earth and the room shakes")]
    [TestCase("N", null, "Cold Passage", "This is a cold and damp corridor")]
    [TestCase("W", null, "Slide Room", "On the south wall of the chamber the letters \"Granite Wall\"")]
    [TestCase("N", null, "Mine Entrance", "You are standing at the entrance of what might have been a coal mine.")]
    [TestCase("W", null, "Squeaky Room", "You may also escape to the east.")]
    [TestCase("inventory", null, "garlic", "skull", "candles", "lantern", "torch (providing", "match", "screw")]
    [TestCase("N", null, "Bat Room", "holding his nose", "exquisite jade figurine")]
    [TestCase("E", null, "Shaft Room", "the chain is a basket")]
    [TestCase("put torch in basket", null, "Done")]
    [TestCase("put screwdriver in basket", null, "Done")]
    [TestCase("turn on lantern", null, "The brass lantern is now on")]
    public async Task Walkthrough(string input, string setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
        {
            var method = GetType().GetMethod(setup);
            if (method == null) throw new ArgumentException("Method " + setup + " doesn't exist");

            // Invoke the method on the current instance
            method.Invoke(this, null);
        }

        await Do(input, expectedResponses);
    }

    public void KillTroll()
    {
        // We can't have the randomness of trying to kill the troll. Let's God-Mode this dude. 
        Repository.GetItem<Troll>().IsDead = true;
    }

    public void GoToRoundRoom()
    {
        // Entering the loud room when it's draining will cause us to flee the room in a random 
        // direction. For the test we need to remove the randomness and end up in the Round Room
        _target.Context.CurrentLocation = Repository.GetLocation<RoundRoom>();
    }

    private async Task Do(string input, params string[] outputs)
    {
        var result = await _target.GetResponse(input);
        Console.WriteLine(result);
        foreach (var output in outputs) result.Should().Contain(output);
    }
}