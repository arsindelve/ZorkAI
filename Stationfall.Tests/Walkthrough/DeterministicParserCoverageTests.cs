using FluentAssertions;
using GameEngine;
using UnitTests;

namespace Stationfall.Tests.Walkthrough;

/// <summary>
///     Pins which walkthrough commands the production <see cref="DeterministicParser" /> cannot resolve by
///     itself. Everything else in the walkthroughs parses with no AI call.
/// </summary>
[TestFixture]
public class DeterministicParserCoverageTests
{
    private static readonly string[] KnownDeclined =
    [
        // Numerals are not declared nouns.
        "type 3", "type 503",

        // No object at all: "up" is a direction, not something to get.
        "get up"
    ];

    [Test]
    public void TheParserDeclinesExactlyTheKnownCommands()
    {
        Repository.Reset();
        var commands = DeterministicParserCoverage.WalkthroughCommands(typeof(WalkthroughTestBase));

        var declined = DeterministicParserCoverage.Declined("Stationfall", commands);

        TestContext.Out.WriteLine($"{commands.Count - declined.Count} of {commands.Count} walkthrough commands parse deterministically.");
        declined.Should().BeEquivalentTo(KnownDeclined,
            "a command missing from this list has stopped parsing deterministically (a regression), and one " +
            "listed here that now parses should be removed from it");
    }

    // A different record, the same outcome: the grammar makes "enter <thing>" the boarding intent, the
    // golden parser a plain verb and noun. The deterministic-first walkthrough replays it.
    private static readonly string[] KnownDisagreements =
    [
        "enter truck: grammar [board truck] vs golden [simple enter | truck | ]"
    ];

    [Test]
    public async Task WhereTheGrammarAndTheGoldenTestParserBothAnswer_TheyAgree()
    {
        Repository.Reset();
        var commands = DeterministicParserCoverage.WalkthroughCommands(typeof(WalkthroughTestBase));

        var disagreements = await DeterministicParserCoverage.Disagreements("Stationfall", commands,
            new TestParser(new Stationfall.GlobalCommand.StationfallGlobalCommandFactory(), "Stationfall"));

        disagreements.Should().BeEquivalentTo(KnownDisagreements,
            "the walkthroughs prove the golden parser's reading of each command; the production grammar " +
            "must read it the same way or decline it");
    }
}
