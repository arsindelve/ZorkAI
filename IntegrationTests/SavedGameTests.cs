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
    public async Task WriteSaveGame_New()
    {
        var name = Guid.NewGuid().ToString();
        var target = new DynamoDbSavedGameRepository();

        var id = await target.SaveGame(null, "bob", name, "Game Data");

        id.Should().NotBeEmpty();

        var allGames = await target.GetSavedGames("bob");
        allGames.Select(s => s.Name).Should().Contain(name);
    }
    
    [Test]
    public async Task WriteSaveGame_Overwrite()
    {
        var name = Guid.NewGuid().ToString();
        var target = new DynamoDbSavedGameRepository();

        var id = await target.SaveGame(null, "bob", name, "Game Data");
        await target.SaveGame(id, "bob", name, "Game Data #2");

        id.Should().NotBeEmpty();

        var result = await target.GetSavedGame(id, "bob");
        result.Should().Be("Game Data #2");
    }


    [Test]
    public async Task GetSaveGame()
    {
        var name = Guid.NewGuid().ToString();
        var target = new DynamoDbSavedGameRepository();

        string id = await target.SaveGame(null, "bob", name, "Game Data");

        var result = await target.GetSavedGame(id, "bob");
        result.Should().Be("Game Data");
    }
}