using FluentAssertions;
using GameEngine;
using ZorkOne.Location;

namespace ZorkOne.Tests.Things;

public class MiscItemTests : EngineTestsBase
{
    [Test]
    [TestCase("i", "An ancient map", "Map")]
    [TestCase("examine map", "The map shows a forest with three clearings", "Map")]
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
    [TestCase("look", "There is an ancient map here", "map")]
    public async Task OnTheGroundTests(string input, string expectedResponse, string itemType)
    {
        var target = GetTarget();

        // Use the passed type to get the item
        Repository.LoadAllItems();
        var item = Repository.GetItem(itemType)!;
        target.Context.CurrentLocation.ItemPlacedHere(item);
        
        var response = await target.GetResponse(input);
        response.Should().Contain(expectedResponse);
    }
}