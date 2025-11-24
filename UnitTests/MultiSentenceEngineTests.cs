using GameEngine;
using Model.AIParsing;
using Model.Intent;
using Model.Movement;
using ZorkOne;
using ZorkOne.GlobalCommand;

namespace UnitTests;

[TestFixture]
public class MultiSentenceEngineTests : EngineTestsBase
{
    [Test]
    public async Task ProcessMultipleSentences_TwoMovements_ExecutesSequentially()
    {
        var target = GetTarget();

        var result = await target.GetResponse("s. look");

        result.Should().Contain("South of House");
        result.Should().Contain("open field");
    }

    [Test]
    public async Task ProcessMultipleSentences_ThreeCommands_AllExecute()
    {
        var target = GetTarget();

        var result = await target.GetResponse("s. e. look");

        result.Should().Contain("South of House");
        result.Should().Contain("Behind House");
        result.Should().Contain("small window");
    }

    [Test]
    public async Task ProcessMultipleSentences_Movement_UpdatesLocation()
    {
        var target = GetTarget();

        await target.GetResponse("s. e");

        target.LocationName.Should().Be("Behind House");
        target.Moves.Should().BeGreaterThan(1);
    }

    [Test]
    public async Task ProcessMultipleSentences_SimpleCommands_EachGetsProcessed()
    {
        var target = GetTarget();

        var result = await target.GetResponse("inventory. score");

        result.Should().Contain("carrying");
        result.Should().Contain("score");
    }

    [Test]
    public async Task ProcessMultipleSentences_SeparatesResponses_WithBlankLines()
    {
        var target = GetTarget();

        var result = await target.GetResponse("look. inventory");

        // Should have blank line separation between responses
        result.Should().Contain(Environment.NewLine + Environment.NewLine);
    }

    [Test]
    public async Task ProcessMultipleSentences_InvalidCommand_ContinuesProcessing()
    {
        var target = GetTarget();

        Mock.Get(Parser)
            .Setup(s => s.DetermineComplexIntentType("xyzzy", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new NullIntent());

        Client
            .Setup(s => s.GenerateNarration(It.IsAny<Model.AIGeneration.Requests.CommandHasNoEffectOperationRequest>(), It.IsAny<string>()))
            .ReturnsAsync("Nothing happens.");

        var result = await target.GetResponse("xyzzy. look");

        result.Should().Contain("Nothing happens");
        result.Should().Contain("West of House");
    }

    [Test]
    public async Task SingleSentence_StillWorks_AsExpected()
    {
        var target = GetTarget();

        var result = await target.GetResponse("s");

        result.Should().Contain("South of House");
        result.Should().NotContain(Environment.NewLine + Environment.NewLine); // No double newlines for single command
    }

    [Test]
    public async Task ProcessMultipleSentences_EmptyInput_ReturnsGeneratedResponse()
    {
        var target = GetTarget();

        Client.Setup(s => s.GenerateNarration(It.IsAny<Model.AIGeneration.Requests.EmptyRequest>(), string.Empty))
            .ReturnsAsync("I beg your pardon?");

        var result = await target.GetResponse("");

        result.Should().Contain("I beg your pardon?");
    }

    [Test]
    public async Task ProcessMultipleSentences_WithPeriodInMiddle_SplitsCorrectly()
    {
        var target = GetTarget();

        var result = await target.GetResponse("look. s");

        result.Should().Contain("West of House");
        result.Should().Contain("South of House");
    }

    [Test]
    public async Task ProcessMultipleSentences_LongChain_ExecutesAll()
    {
        var target = GetTarget();

        var result = await target.GetResponse("s. e. n. look");

        // s -> South of House
        // e -> Behind House
        // n -> North of House
        // look -> shows North of House description

        result.Should().Contain("South of House");
        result.Should().Contain("Behind House");
        result.Should().Contain("North of House");
        target.LocationName.Should().Be("North of House");
    }

    [Test]
    public async Task ProcessMultipleSentences_IncrementsMovesForEach()
    {
        var target = GetTarget();

        var initialMoves = target.Moves;

        await target.GetResponse("s. e. look");

        // s and e are moves, look is also a turn
        target.Moves.Should().Be(initialMoves + 3);
    }

    [Test]
    public async Task ProcessMultipleSentences_SystemCommand_ProcessedFirst()
    {
        var target = GetTarget();

        var result = await target.GetResponse("brief. look");

        result.Should().Contain("Okay");
        result.Should().Contain("West of House");
    }

    [Test]
    public async Task ProcessMultipleSentences_TrailingPeriod_HandledGracefully()
    {
        var target = GetTarget();

        var result = await target.GetResponse("s. e.");

        result.Should().Contain("South of House");
        result.Should().Contain("Behind House");
    }

    [Test]
    public async Task ProcessMultipleSentences_OnlyPeriods_ReturnsEmpty()
    {
        var target = GetTarget();

        Client.Setup(s => s.GenerateNarration(It.IsAny<Model.AIGeneration.Requests.EmptyRequest>(), string.Empty))
            .ReturnsAsync("I beg your pardon?");

        var result = await target.GetResponse("...");

        result.Should().Contain("I beg your pardon?");
    }
}
