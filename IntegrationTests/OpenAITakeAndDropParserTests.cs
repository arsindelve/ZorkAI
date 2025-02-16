using GameEngine;
using Model.Interface;
using Model.Location;
using Moq;
using OpenAI;
using ZorkOne.Item;
using ZorkOne.Location;

namespace IntegrationTests;

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
}