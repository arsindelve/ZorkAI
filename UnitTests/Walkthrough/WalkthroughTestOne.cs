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
        await Do("W", "Living Room", "A battery-powered brass lantern", "and a large oriental rug in the center of the room");
        await Do("take lamp", "Taken");
        await Do("move rug", "With a great effort,");
        await Do("open trap door", "The door reluctantly opens to reveal a rickety staircase descending into darkness");
        await Do("turn on lantern", "The brass lantern is now on.");
        await Do("go down", "The trap door crashes shut", "You are in a dark and damp cellar with a narrow passageway");
        await Do("S", "You are on the east edge of a chasm");
        await Do("E", "Most of the paintings have been stolen by", "Fortunately, there is still one chance for you to be a vandal");
        await Do("take painting", "Taken");
        await Do("N", "This appears to have been an artist's studio.", "Loosely attached to a wall is a small piece of paper.");
        await Do("Up", "Kitchen");
        await Do("Up", "Attic", "On a table is a nasty-looking knife", "A large coil of rope is lying in the corner");
        await Do("take knife", "Taken");
        await Do("take rope", "Taken");
        await Do("go down", "Kitchen");
        await Do("W", "Above the trophy case hangs an elvish sword of great antiquity.", "Living Room");
        await Do("open case", "Opened");
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
        await Do("E", "You are at the periphery of a large dome, which forms the ceiling of another room below", "Dome Room");
        await Do("tie rope to railing", "The rope drops over the side and comes within ten feet of the floor.");
        await Do("go down", "Torch Room", "Sitting on the pedestal is a flaming torch, made of ivory.");
        await Do("S", "Temple");
        
        // Get the coffin 
        
        await Do("S", "Altar");
        await Do("pray", "Forest", "sunlight");
        
    }

    private async Task Do(string input, params string[] outputs)
    {
        var result = await _target.GetResponse(input);
        foreach (var output in outputs) result.Should().Contain(output);
    }
}