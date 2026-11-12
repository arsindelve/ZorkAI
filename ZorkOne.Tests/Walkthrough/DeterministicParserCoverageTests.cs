using FluentAssertions;
using GameEngine;
using UnitTests;

namespace ZorkOne.Tests.Walkthrough;

/// <summary>
///     Pins which walkthrough commands the production <see cref="DeterministicParser" /> cannot resolve by
///     itself. Everything else in the walkthroughs parses with no AI call.
/// </summary>
[TestFixture]
public class DeterministicParserCoverageTests
{
    private static readonly string[] KnownDeclined =
    [
        // Never reach the verb/noun grammar in play: quantified take/drop is a global command, and the
        // pronoun is rewritten before parsing.
        "take all", "get all", "drop all", "read it",

        // Verbs the parser does not know yet ("get in" as a boarding phrase, "leave", "cross").
        "get in the boat", "leave boat", "cross the rainbow",

        // A shape outside the grammar: the preposition comes straight after the verb.
        "dig in sand with shovel",

        // Nouns no item or location declares ("bubble"), and a spoken word that is not a noun at all.
        "examine bubble", "take bubble", "say temple"
    ];

    [Test]
    public void TheParserDeclinesExactlyTheKnownCommands()
    {
        Repository.Reset();
        var commands = DeterministicParserCoverage.WalkthroughCommands(typeof(WalkthroughTestBase));

        var declined = DeterministicParserCoverage.Declined("ZorkOne", commands);

        TestContext.Out.WriteLine($"{commands.Count - declined.Count} of {commands.Count} walkthrough commands parse deterministically.");
        declined.Should().BeEquivalentTo(KnownDeclined,
            "a command missing from this list has stopped parsing deterministically (a regression), and one " +
            "listed here that now parses should be removed from it");
    }
}
