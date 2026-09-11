using FluentAssertions;
using GameEngine;

namespace UnitTests.SelfHosted;

/// <summary>
///     Round-trip tests for the file-backed saved-game repository used in self-hosted mode
///     (issue #383) — the replacement for DynamoDbSavedGameRepository that lets the game backends
///     serve save/restore with no AWS. Uses an isolated folder under the test work directory,
///     recreated per test, with the id generator and clock injected so every run is deterministic.
/// </summary>
public class FileSavedGameRepositoryTests
{
    private const string Table = "zork1_savegame";

    private static readonly DateTime FixedNow = new(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);

    private string _baseDirectory = null!;
    private FileSavedGameRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _baseDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "FileSavedGameRepositoryTests");
        if (Directory.Exists(_baseDirectory))
            Directory.Delete(_baseDirectory, true);

        _repository = new FileSavedGameRepository(
            _baseDirectory,
            () => Guid.Parse("11111111-2222-3333-4444-555555555555"),
            () => FixedNow);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_baseDirectory))
            Directory.Delete(_baseDirectory, true);
    }

    [Test]
    public async Task Should_ReturnNull_When_SavedGameDoesNotExist()
    {
        var data = await _repository.GetSavedGame("no-such-id", "session-1", Table);

        data.Should().BeNull();
    }

    [Test]
    public async Task Should_RoundTripASavedGame()
    {
        var id = await _repository.SaveGame(null, "session-1", "Before the troll", "base64-game-data", Table);

        var data = await _repository.GetSavedGame(id, "session-1", Table);

        data.Should().Be("base64-game-data");
    }

    [Test]
    public async Task Should_GenerateAnId_When_NoneIsSupplied()
    {
        var id = await _repository.SaveGame(null, "session-1", "Auto", "data", Table);

        id.Should().Be("11111111-2222-3333-4444-555555555555");
    }

    [Test]
    public async Task Should_KeepTheSuppliedId_And_Overwrite_When_SavingAgain()
    {
        var id = await _repository.SaveGame("my-slot", "session-1", "First", "first-data", Table);
        var sameId = await _repository.SaveGame("my-slot", "session-1", "Second", "second-data", Table);

        sameId.Should().Be("my-slot");
        (await _repository.GetSavedGame("my-slot", "session-1", Table)).Should().Be("second-data");

        var saves = await _repository.GetSavedGames("session-1", Table);
        saves.Should().HaveCount(1);
        saves.Single().Name.Should().Be("Second");
    }

    [Test]
    public async Task Should_ListEverySaveForTheSession_WithNameAndTimestamp()
    {
        await _repository.SaveGame("a", "session-1", "Early game", "data-a", Table);
        await _repository.SaveGame("b", "session-1", "Late game", "data-b", Table);

        var saves = await _repository.GetSavedGames("session-1", Table);

        saves.Should().HaveCount(2);
        saves.Select(s => s.Id).Should().BeEquivalentTo("a", "b");
        saves.Select(s => s.Name).Should().BeEquivalentTo("Early game", "Late game");
        saves.Should().OnlyContain(s => s.SavedOn == FixedNow);
    }

    [Test]
    public async Task Should_ReturnEmptyList_When_TheSessionHasNoSaves()
    {
        var saves = await _repository.GetSavedGames("nobody", Table);

        saves.Should().BeEmpty();
    }

    [Test]
    public async Task Should_KeepSavesSeparate_BySession()
    {
        await _repository.SaveGame("same-id", "session-1", "Mine", "mine", Table);
        await _repository.SaveGame("same-id", "session-2", "Yours", "yours", Table);

        (await _repository.GetSavedGame("same-id", "session-1", Table)).Should().Be("mine");
        (await _repository.GetSavedGame("same-id", "session-2", Table)).Should().Be("yours");
        (await _repository.GetSavedGames("session-1", Table)).Should().HaveCount(1);
    }

    [Test]
    public async Task Should_KeepSavesSeparate_ByTableName()
    {
        await _repository.SaveGame("same-id", "session-1", "Zork", "zork-data", Table);
        await _repository.SaveGame("same-id", "session-1", "Planetfall", "pf-data", "planetfall_savegame");

        (await _repository.GetSavedGame("same-id", "session-1", Table)).Should().Be("zork-data");
        (await _repository.GetSavedGame("same-id", "session-1", "planetfall_savegame")).Should().Be("pf-data");
    }

    [Test]
    public async Task Should_DeleteASavedGame()
    {
        await _repository.SaveGame("doomed", "session-1", "Doomed", "data", Table);

        await _repository.DeleteSavedGameAsync("doomed", "session-1", Table);

        (await _repository.GetSavedGame("doomed", "session-1", Table)).Should().BeNull();
        (await _repository.GetSavedGames("session-1", Table)).Should().BeEmpty();
    }

    [Test]
    public async Task Should_NotThrow_When_DeletingASaveThatIsNotThere()
    {
        var deleting = async () => await _repository.DeleteSavedGameAsync("ghost", "session-1", Table);

        await deleting.Should().NotThrowAsync();
    }

    [Test]
    public async Task Should_SkipACorruptedSave_When_ListingTheRest()
    {
        await _repository.SaveGame("good", "session-1", "Readable", "data", Table);

        // A half-written or hand-edited file must not make the whole save list unreadable and lock
        // the player out of the saves that are fine.
        var corrupted = Path.Combine(_baseDirectory, Table, "session-1", "broken.savedgame");
        await File.WriteAllTextAsync(corrupted, "{ this is not json");

        var saves = await _repository.GetSavedGames("session-1", Table);

        saves.Should().HaveCount(1);
        saves.Single().Id.Should().Be("good");
    }

    [Test]
    public async Task Should_HandleIdsAndSessions_WithFilesystemHostileCharacters()
    {
        await _repository.SaveGame("id/with:slash", "user/one*two", "Hostile", "data", Table);

        var data = await _repository.GetSavedGame("id/with:slash", "user/one*two", Table);

        data.Should().Be("data");
    }
}
