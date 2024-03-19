﻿using ZorkOne;

namespace UnitTests.Walkthrough;

public sealed class WalkthroughTestOne : EngineTestsBase
{
    private GameEngine<ZorkI> _target;

    public WalkthroughTestOne()
    {
        _target = GetTarget();
    }

    [Test]
    //[Ignore("I need to see coverage from elsewhere")]
    public async Task Walkthrough()
    {
        // https://web.mit.edu/marleigh/www/portfolio/Files/zork/transcript.html

        _target = GetTarget();

        await Do("open mailbox", "Opening the small mailbox reveals a leaflet.");
        await Do("read leaflet", "ZORK is a game of adventure, danger, and low cunning. In it you will explore");
        await Do("drop leaflet", "Dropped");
        await Do("S", "South of House");
        await Do("E", "Behind House");
        await Do("open window", "With great effort, you open the window far enough to allow entry.");
        await Do("In", "Kitchen", "A quantity of water", "smelling of hot peppers.");
        await Do("W", "Living Room", "A battery-powered brass lantern", "and a large oriental rug in the center of");
        await Do("take lamp", "Taken");
        await Do("move rug", "With a great effort,");
        await Do("open trap door", "The door reluctantly opens to reveal a rickety staircase descending into darkness");
        await Do("turn on lantern", "The brass lantern is now on.");
        await Do("go down", "The trap door crashes shut", "You are in a dark and damp cellar with a narrow passageway");
        await Do("S", "You are on the east edge of a chasm");
        await Do("E", "Most of the paintings have been stolen by", "Fortunately, there is still one chance for you to be");
        await Do("take painting", "Taken");
        await Do("N", "This appears to have been an artist's studio.","Loosely attached to a wall is a small");
        await Do("Up", "Kitchen");
        await Do("Up", "Attic", "On a table is a nasty-looking knife", "A large coil of rope is lying in the corner");
        await Do("take knife", "Taken");
        await Do("take rope", "Taken");
        await Do("go down", "Kitchen");
        await Do("W", "Above the trophy case hangs an elvish sword of great antiquity.", "Living Room");
        await Do("open case", "Opened");
        
        // Painting
        await Do("put painting inside case", "Done");
        
        await Do("drop knife", "Dropped");
        await Do("take sword", "Taken");
        await Do("open trap door", "The door reluctantly opens to reveal a rickety staircase descending into darkness");
        await Do("go down", "The trap door crashes shut", "faint blue glow");
        await Do("N", "Bloodstains", "very brightly", "nasty-looking troll");
        // TODO: Kill the troll
        await Do("drop sword", "Dropped");
        await Do("E", "This is a narrow east-west passageway");
        await Do("E", "This is a circular stone room with passages in all direction", "Round Room");
        await Do("SE", "There are old engravings on the walls here", "Engravings Cave");
        await Do("E", "periphery of a large dome, which forms the ceiling of another room below", "Dome Room");
        await Do("tie rope to railing", "The rope drops over the side and comes within ten feet of the floor.");
        await Do("go down", "Torch Room", "Sitting on the pedestal is a flaming torch, made of ivory.");
        await Do("S", "Temple");
        await Do("E", "Egyptian Room", "The solid-gold coffin used for the burial of Ramses II is here.");
        await Do("take coffin", "Taken");
        await Do("W", "Temple");
        await Do("S", "Altar");
        await Do("pray", "Forest", "sunlight");
        await Do("turn off lantern", "The brass lantern is now off");
        await Do("S", "This is a dimly lit forest");
        await Do("N", "Clearing");
        await Do("E", "Canyon View", "stretching for miles around");
        await Do("Down", "Rocky Ledge", "which appears climbable");
        await Do("Down", "Canyon Bottom", "Aragain Falls");
        await Do("N", "End of Rainbow", "The beach is narrow due to the presence of the White Cliffs.");
        await Do("drop coffin", "Dropped");
        await Do("open coffin", "The gold coffin opens", "sceptre is ornamented with colored enamel, and tapers to a ");
        await Do("take sceptre", "Taken");
        // Wave the sceptre, get the gold

        await Do("take coffin", "Taken");
        await Do("SW", "Canyon Bottom");
        await Do("Up", "Rocky Ledge");
        await Do("Up", "Canyon View");
        await Do("NW", "Clearing");
        await Do("W", "Behind House");
        await Do("In", "Kitchen", "A bottle is sitting on the table", "elongated");
        await Do("open sack", "Opening the brown sack reveals a lunch, and a clove of garlic");
        await Do("take garlic", "Taken");
        await Do("W", "Living Room", "Your collection of treasures consists of", "A painting");

        // Gold, sceptre and coffin
        await Do("put coffin inside case", "Done");
        await Do("put sceptre inside case", "Done");
        
        await Do("open trap door", "The door reluctantly opens to reveal a rickety staircase descending into darkness");
        await Do("turn on lantern", "The brass lantern is now on.");
        await Do("go down", "The trap door crashes shut", "Cellar");
        await Do("N", "Bloodstains", "Troll Room");
        await Do("E", "This is a narrow east-west passageway");
        
    }

    private async Task Do(string input, params string[] outputs)
    {
        var result = await _target.GetResponse(input);
        foreach (var output in outputs) result.Should().Contain(output);
    }
}