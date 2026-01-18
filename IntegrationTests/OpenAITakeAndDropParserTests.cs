using GameEngine;
using Model.Interface;
using Model.Location;
using Moq;
using OpenAI;
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
        // Navigate up from the test assembly to find the solution root
        var assemblyLocation = Path.GetDirectoryName(typeof(OpenAITakeAndDropParserTests).Assembly.Location)!;
        var directory = new DirectoryInfo(assemblyLocation);

        // Walk up until we find the .env file or reach the root
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, ".env")))
        {
            directory = directory.Parent;
        }

        if (directory != null)
        {
            var resolvedPath = Path.Combine(directory.FullName, ".env");
            Console.WriteLine($"Loading .env from: {resolvedPath}");
            Env.Load(resolvedPath, new LoadOptions());
        }
        else
        {
            Console.WriteLine("Warning: .env file not found in solution hierarchy");
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
        var response = await target.GetListOfItemsToTake(command, locationObjectDescription);
        response.Length.Should().Be(nouns.Length);
        foreach (var noun in nouns)
        {
            response.Should().Contain(noun);
        }
        
    }
    
    [Test]
    [TestCase("take the ID card", "ID card")]
    [TestCase("take ID card", "ID card")]
    [TestCase("take the ID card and the key", "ID card", "key")]
    public async Task TakeCompoundNounItems(string command, params string[] nouns)
    {
        string locationDescription = """
                                     Storage Room
                                     A small storage room with metal shelves.
                                     On a shelf you see an ID card and a key.
                                     """;

        var target = new OpenAITakeAndDropListParser(null);
        var response = await target.GetListOfItemsToTake(command, locationDescription);
        response.Length.Should().Be(nouns.Length);
        foreach (var noun in nouns)
        {
            response.Any(r => r.Equals(noun, StringComparison.OrdinalIgnoreCase)).Should().BeTrue($"Expected to find '{noun}'");
        }
    }

    [Test]
    [TestCase("drop the leaflet", "leaflet")]
    [TestCase("drop the weapons", "sword", "nasty knife")]
    [TestCase("drop the leaflet and the rope", "leaflet", "rope")]
    [TestCase("drop the sack, the knife and the rope", "brown sack", "nasty knife", "rope")]
    [TestCase("drop everything", "brown sack", "nasty knife", "rope", "brass lantern", "sword", "glass bottle", "leaflet")]
    [TestCase("drop all except the leaflet", "brown sack", "nasty knife", "rope", "brass lantern", "sword", "glass bottle")]
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
        var response = await target.GetListOfItemsToDrop(command, inventory);
        response.Length.Should().Be(nouns.Length);
        foreach (var noun in nouns)
        {
            response.Should().Contain(noun);
        }
    }
}