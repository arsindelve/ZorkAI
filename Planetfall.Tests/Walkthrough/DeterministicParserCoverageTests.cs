using FluentAssertions;
using GameEngine;
using UnitTests;

namespace Planetfall.Tests.Walkthrough;

/// <summary>
///     Pins which walkthrough commands the production <see cref="DeterministicParser" /> cannot resolve by
///     itself. Everything else in the walkthroughs parses with no AI call.
/// </summary>
[TestFixture]
public class DeterministicParserCoverageTests
{
    private static readonly string[] KnownDeclined =
    [
        // Never reach the verb/noun grammar in play: conversation is routed to the character first, a bare
        // noun phrase is a disambiguation reply, and quantified drop is a global command.
        "floyd, go north", "floyd, take board", "floyd, are you okay", "ask floyd about the card", "lab door",
        "drop all",

        // Verbs the parser does not know yet.
        "pull lever", "collapse ladder", "deactivate floyd", "deactivate terminal", "lie down in bed",
        "slide kitchen access card through slot", "slide upper access card through slot",
        "slide lower access card through slot", "slide shuttle access card through slot",
        "slide teleportation card through slot", "slide mini card through slot",

        // Numerals and ordinals are not declared nouns.
        "press 2", "press 3", "type 0", "type 1", "type 6", "type 384", "type 385", "set laser to 1",
        "set laser to 2", "press 0 on the terminal", "key 4 on the terminal", "take second",

        // Shapes outside the grammar: a preposition it does not connect nouns with ("across", "off"), or
        // one that comes straight after the verb.
        "place ladder across rift", "throw laser off strip", "jump into rift"
    ];

    [Test]
    public void TheParserDeclinesExactlyTheKnownCommands()
    {
        Repository.Reset();
        var commands = DeterministicParserCoverage.WalkthroughCommands(typeof(WalkthroughTestBase));

        var declined = DeterministicParserCoverage.Declined("Planetfall", commands);

        TestContext.Out.WriteLine($"{commands.Count - declined.Count} of {commands.Count} walkthrough commands parse deterministically.");
        declined.Should().BeEquivalentTo(KnownDeclined,
            "a command missing from this list has stopped parsing deterministically (a regression), and one " +
            "listed here that now parses should be removed from it");
    }
}
