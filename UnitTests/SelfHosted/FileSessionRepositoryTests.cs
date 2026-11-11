using FluentAssertions;
using GameEngine;

namespace UnitTests.SelfHosted;

/// <summary>
///     Round-trip tests for the file-backed session repository used in self-hosted mode
///     (issue #383). Uses an isolated folder under the test work directory, recreated per test.
/// </summary>
public class FileSessionRepositoryTests
{
    private string _baseDirectory = null!;
    private FileSessionRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _baseDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "FileSessionRepositoryTests");
        if (Directory.Exists(_baseDirectory))
            Directory.Delete(_baseDirectory, true);
        _repository = new FileSessionRepository(_baseDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_baseDirectory))
            Directory.Delete(_baseDirectory, true);
    }

    [Test]
    public async Task Should_ReturnNull_When_SessionDoesNotExist()
    {
        var state = await _repository.GetSessionState("nobody", "zork_session");

        state.Should().BeNull();
    }

    [Test]
    public async Task Should_RoundTripSessionState()
    {
        await _repository.WriteSessionState("my-machine8", "base64-game-data", "zork_session");

        var state = await _repository.GetSessionState("my-machine8", "zork_session");

        state.Should().Be("base64-game-data");
    }

    [Test]
    public async Task Should_OverwriteSessionState_When_WrittenTwice()
    {
        await _repository.WriteSessionState("my-machine8", "turn-one", "zork_session");
        await _repository.WriteSessionState("my-machine8", "turn-two", "zork_session");

        var state = await _repository.GetSessionState("my-machine8", "zork_session");

        state.Should().Be("turn-two");
    }

    [Test]
    public async Task Should_KeepSessionsSeparate_ByTableName()
    {
        await _repository.WriteSessionState("my-machine8", "zork-data", "zork_session");
        await _repository.WriteSessionState("my-machine8", "planetfall-data", "planetfall_session");

        (await _repository.GetSessionState("my-machine8", "zork_session")).Should().Be("zork-data");
        (await _repository.GetSessionState("my-machine8", "planetfall_session")).Should().Be("planetfall-data");
    }

    [Test]
    public async Task Should_AppendSteps_And_ReadThemBackInOrder()
    {
        await _repository.WriteSessionStep("my-machine8", 1, "open mailbox", "Opening the small mailbox reveals a leaflet.", "zork_session");
        await _repository.WriteSessionStep("my-machine8", 2, "take leaflet", "Taken.", "zork_session");

        var steps = await _repository.GetSessionStepsAsText("my-machine8", "zork_session");

        steps.Should().Contain("[1] > open mailbox");
        steps.Should().Contain("[2] > take leaflet");
        steps.IndexOf("open mailbox", StringComparison.Ordinal)
            .Should().BeLessThan(steps.IndexOf("take leaflet", StringComparison.Ordinal));
    }

    [Test]
    public async Task Should_ReturnEmptySteps_When_NoneWereWritten()
    {
        var steps = await _repository.GetSessionStepsAsText("nobody", "zork_session");

        steps.Should().BeEmpty();
    }

    [Test]
    public async Task Should_HandleSessionIds_WithFilesystemHostileCharacters()
    {
        var hostileId = "user/one:two*three";

        await _repository.WriteSessionState(hostileId, "data", "zork_session");

        (await _repository.GetSessionState(hostileId, "zork_session")).Should().Be("data");
    }

    [Test]
    public void Should_SanitizeInvalidFileNameCharacters()
    {
        FileSessionRepository.Sanitize("a/b\\c").Should().NotContain("/").And.NotContain("\\");
        FileSessionRepository.Sanitize("plain-name8").Should().Be("plain-name8");
        FileSessionRepository.Sanitize("  ").Should().Be("_");
    }
}
