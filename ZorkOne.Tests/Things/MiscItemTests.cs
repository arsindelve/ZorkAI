using FluentAssertions;
using GameEngine;
using ZorkOne.Location;

namespace ZorkOne.Tests.Things;

public class MiscItemTests : EngineTestsBase
{
    [Test]
    [TestCase("i", "An ancient map", "Map")]
    [TestCase("i", "A rusty knife", "rusty")]
    [TestCase("examine map", "The map shows a forest with three clearings", "Map")]
    [TestCase("i", "A tour guidebook", "guidebook")]
    [TestCase("read guidebook", "You will notice on your right", "guidebook")]
    [TestCase("examine guidebook", "You will notice on your right", "guidebook")]
    [TestCase("i", "A burned-out lantern", "burned-out lantern")]
    [TestCase("i", "A tan label", "label")]
    [TestCase("read label", "Sailor!", "label")]
    [TestCase("examine label", "Sailor!", "label")]
    [TestCase("i", "A bloody axe", "axe")]
    [TestCase("i", "A broken timber", "timber")]
    [TestCase("examine buoy", "The buoy is closed", "buoy")]
    public async Task InInventoryTests(string input, string expectedResponse, string itemType)
    {
        var target = GetTarget();

        Repository.LoadAllItems();
        var item = Repository.GetItem(itemType);
        target.Context.Take(item);

        var response = await target.GetResponse(input);
        response.Should().Contain(expectedResponse);
    }

    [Test]
    [TestCase("look", "There is a red buoy here (probably a warning).", "buoy")]
    [TestCase("look", "There is an ancient map here", "map")]
    [TestCase("look", "In the corner of the room on the ceiling is a large vampire bat who is obviously deranged and holding his nose", "bat")]
    [TestCase("look", "There is a large emerald here", "emerald")]
    [TestCase("look", "Egypt itself", "sceptre")]
    [TestCase("examine chain", "The chain secures a basket within the shaft", "chain")]
    [TestCase("sit on rug", "As you sit, you notice an irregularity underneath it", "rug")]
    [TestCase("sit on rug", "magic carpet", "rug", "move rug")]
    [TestCase("move rug", "Having moved the carpet", "rug", "move rug")]
    [TestCase("look under rug", "trap door", "rug")]
    [TestCase("look under rug", "nothing but dust", "rug", "move rug")]
    [TestCase("look", "Loosely attached to a wall is a small piece of paper", "manual")]
    [TestCase("look", "The deceased adventurer's useless lantern is here.", "useless")]
    [TestCase("look", "Some guidebooks entitled \"Flood Control Dam #3\" are on the reception desk.", "guidebook")]
    public async Task OnTheGroundTests_NeverPickedUp(string input, string expectedResponse, string itemType, string? precondition = null)
    {
        var target = GetTarget();

        // Use the passed type to get the item
        Repository.LoadAllItems();
        var item = Repository.GetItem(itemType)!;
        target.Context.CurrentLocation.ItemPlacedHere(item);

        if (!string.IsNullOrEmpty(precondition))
            await target.GetResponse(precondition);
        
        var response = await target.GetResponse(input);
        response.Should().Contain(expectedResponse);
    }

    [Test]
    [TestCase("look", "There is a red buoy here.", "buoy")]
    [TestCase("look", "There is a crystal trident here", "trident")]
    [TestCase("look", "There is a huge diamond here", "diamond")]
    [TestCase("look", "There is a stiletto here", "stiletto")]
    [TestCase("look", "There is an ancient map here", "map")]
    [TestCase("look", "sharp point", "sceptre")]
    [TestCase("look", "There is a large emerald here", "emerald")]
    [TestCase("look", "There is a tour guidebook here", "guidebook")]
    [TestCase("look", "There is a burned-out lantern here.", "useless")]
    [TestCase("look", "There is a ZORK owner's manual here.", "manual")]
    [TestCase("look", "There is a rusty knife", "rusty")]
    public async Task OnTheGroundTests_AfterPickedUp(string input, string expectedResponse, string itemType)
    {
        var target = GetTarget();

        Repository.LoadAllItems();
        var item = Repository.GetItem(itemType)!;
        target.Context.ItemPlacedHere(item);
        await target.GetResponse($"drop {itemType}");

        var response = await target.GetResponse(input);
        response.Should().Contain(expectedResponse);
    }

    [Test]
    [TestCase("The chain is secure.", "chain")]
    [TestCase("The cage is securely fastened to the iron chain", "basket")]
    [TestCase("You can't reach him; he's on the ceiling.", "bat")]
    [TestCase("irregularity", "rug")]
    [TestCase("extremely heavy", "rug", "move rug")]
    [TestCase("You can't be serious.", "ladder")]
    [TestCase("A ghost appears in the room and is appalled at your desecration of the remains of a fellow adventurer", "skeleton")]
    public async Task CannotBeTaken(string expectedResponse, string itemType, string? precondition = null)
    {
        var target = GetTarget();

        Repository.LoadAllItems();
        var item = Repository.GetItem(itemType)!;
        target.Context.CurrentLocation.ItemPlacedHere(item);
        if (!string.IsNullOrEmpty(precondition))
            await target.GetResponse(precondition);
        
        var response = await target.GetResponse($"take {itemType}");
        response.Should().Contain(expectedResponse);
    }
}