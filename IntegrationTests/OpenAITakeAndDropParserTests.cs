using GameEngine;
using Model.Interface;
using Model.Location;
using Moq;
using OpenAI;
using ZorkOne.Item;
using ZorkOne.Location;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Parallelizable(ParallelScope.Children)]
public class OpenAITakeAndDropParserTests
{
    private readonly object _lockObject = new();

    [SetUp]
    public void Setup()
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var resolvedPath = Path.Combine(homePath, "RiderProjects/ZorkAI/.env");
        Console.WriteLine($"Resolved path: {resolvedPath}");
        if (!File.Exists(resolvedPath))
        {
            Console.WriteLine($"File does not exist at path: {resolvedPath}");
        }
        else
        {
            Console.WriteLine($"File exists at path: {resolvedPath}");
            Env.Load(resolvedPath, new LoadOptions());
        }
    }

    [Test]
    [TestCase("take the screwdriver", "screwdriver")]
    [TestCase("take all the tools but don't take the tube", "screwdriver", "wrench", "tool chests")]
    [TestCase("take the screwdriver and the wrench", "screwdriver", "wrench")]
    [TestCase("take screwdriver, wrench", "screwdriver", "wrench")]
    [TestCase("pick up everything except the tube", "screwdriver", "tool chests", "wrench")]
    [TestCase("pick up everything except the tube and the tool chests, I don't want those", "screwdriver", "wrench")]
    public async Task TakeListOfItems(string command, params string[] nouns)
    {
        string locationObjectDescription;

        lock (_lockObject)
        {
            Repository.Reset();
            var locationObject = (ILocation)Activator.CreateInstance(typeof(MaintenanceRoom))!;
            locationObject.Init();
            locationObjectDescription = locationObject.GetDescription(Mock.Of<IContext>());
        }

        var target = new OpenAITakeAndDropListParser(null);
        var response = await target.GetListOfItemsToTake(command, locationObjectDescription, string.Empty);
        response.Length.Should().Be(nouns.Length);
        foreach (var noun in nouns)
        {
            response.Should().Contain(noun);
        }
        
    }
    
    [Test]
    [TestCase("drop the leaflet", "leaflet")]
    [TestCase("drop the leaflet and the rope", "leaflet", "rope")]
    [TestCase("drop the sack, the knife and the rope", "sack", "knife", "rope")]
    [TestCase("drop everything", "brown sack", "nasty knife", "rope", "brass lantern", "sword", "glass bottle", "leaflet")]
    [TestCase("drop all except the leaflet", "brown sack", "nasty knife", "rope", "brass lantern", "sword", "glass bottle")]
    [TestCase("drop everything except don't drop the leaflet", "brown sack", "nasty knife", "rope", "brass lantern", "sword", "glass bottle")]
    [TestCase("drop all but the leaflet and the bottle", "brown sack", "nasty knife", "rope", "brass lantern", "sword")]
    [TestCase("drop all except leaflet, bottle", "brown sack", "nasty knife", "rope", "brass lantern", "sword")]
    public async Task DropListOfItems(string command, params string[] nouns)
    {
        string inventory = """
                           You are carrying:
                               A rope
                               A nasty knife
                               A brass lantern (providing light)
                               A sword
                               A glass bottle
                               The glass bottle contains:
                                 A quantity of water
                               A brown sack
                               A leaflet
                           """;

        var target = new OpenAITakeAndDropListParser(null);
        var response = await target.GetListOfItemsToDrop(command, inventory, string.Empty);
        response.Length.Should().Be(nouns.Length);
        foreach (var noun in nouns)
        {
            response.Should().Contain(noun);
        }
    }
}