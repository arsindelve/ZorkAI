using DynamoDb;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Parallelizable(ParallelScope.Children)]
[Ignore("")]
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

        var id = await target.SaveGame(null, "bob", name, "GameEngine Data", "zork_savegame");

        id.Should().NotBeEmpty();

        var allGames = await target.GetSavedGames("bob", "zork_savegame");
        allGames.Select(s => s.Name).Should().Contain(name);
    }

    [Test]
    public async Task WriteSaveGame_Overwrite()
    {
        var name = Guid.NewGuid().ToString();
        var target = new DynamoDbSavedGameRepository();

        var id = await target.SaveGame(null, "bob", name, "GameEngine Data", "zork_savegame");
        await target.SaveGame(id, "bob", name, "GameEngine Data #2", "zork_savegame");

        id.Should().NotBeEmpty();

        var result = await target.GetSavedGame(id, "bob", "zork_savegame");
        result.Should().Be("GameEngine Data #2");
    }


    [Test]
    public async Task GetSaveGame()
    {
        var name = Guid.NewGuid().ToString();
        var target = new DynamoDbSavedGameRepository();

        var id = await target.SaveGame(null, "bob", name, "GameEngine Data", "zork_savegame");

        var result = await target.GetSavedGame(id, "bob", "zork_savegame");
        result.Should().Be("GameEngine Data");
    }
}