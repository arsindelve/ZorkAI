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

        // Verbs no handler declares: "get in" as a boarding phrase, and "leave" (leaving the boat is the
        // disembark intent, which only the AI produces).
        "get in the boat", "leave boat",

        // A declared verb that no item or room owns: the rainbow's verbs live on a shared helper class, so
        // the grammar cannot tie "cross" to the rainbow and does not vouch for it.
        "cross the rainbow",

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

    // Different records, the same outcome - each is replayed in a deterministic-first walkthrough, which
    // is what proves it.
    private static readonly string[] KnownDisagreements =
    [
        // "galley" is a declared alias of the kitchen; the golden parser models the AI renaming it.
        "go to the galley: grammar [goto galley] vs golden [goto kitchen]",

        // Two nouns of the same object (the pile of plastic that becomes the boat).
        "inflate plastic with pump: grammar [multi inflate | plastic | with | pump] vs golden [multi inflate | boat | with | pump]",

        // The golden parser leaves a stray article in the adverb slot.
        "light a match: grammar [simple light | match | ] vs golden [simple light | match | a]",
        "press the yellow button: grammar [simple press | yellow button | ] vs golden [simple press | yellow button | the]",

        // "turn on X": the grammar gives the canonical verb, the golden parser "turn" plus an adverb.
        // The same processor answers both.
        "turn off lantern: grammar [simple deactivate | lantern | ] vs golden [simple turn | lamp | off]",
        "turn on lantern: grammar [simple activate | lantern | ] vs golden [simple turn | lamp | on]"
    ];

    [Test]
    public async Task WhereTheGrammarAndTheGoldenTestParserBothAnswer_TheyAgree()
    {
        Repository.Reset();
        var commands = DeterministicParserCoverage.WalkthroughCommands(typeof(WalkthroughTestBase));

        var disagreements = await DeterministicParserCoverage.Disagreements("ZorkOne", commands,
            new TestParser(new ZorkOne.GlobalCommand.ZorkOneGlobalCommandFactory(), "ZorkOne"));

        disagreements.Should().BeEquivalentTo(KnownDisagreements,
            "the walkthroughs prove the golden parser's reading of each command; the production grammar " +
            "must read it the same way or decline it");
    }
}
