using DynamoDb;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Parallelizable(ParallelScope.Children)]
public class SavedGameTests
{
    [SetUp]
    public void Setup()
    {
        Env.Load("/Users/michael/RiderProjects/ZorkAI/.env",
            new LoadOptions());
    }

    [Test]
    public async Task WriteSaveGame()
    {
        var name = Guid.NewGuid().ToString();
        var target = new DynamoDbSavedGameRepository();

        var id = await target.SaveGame("bob", name, "Game Data");

        id.Should().NotBeEmpty();

        var allGames = await target.GetSavedGames("bob");
        allGames.Select(s => s.Name).Should().Contain(name);
    }

    [Test]
    public async Task GetSaveGame()
    {
        var name = Guid.NewGuid().ToString();
        var target = new DynamoDbSavedGameRepository();

        string id = await target.SaveGame("bob", name, "Game Data");

        var result = await target.GetSavedGame(id, "bob");
        result.Should().Be("Game Data");
    }
}