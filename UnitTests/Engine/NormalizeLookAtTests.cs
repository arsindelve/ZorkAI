using FluentAssertions;
using GameEngine;
using ZorkOne;

namespace UnitTests.Engine;

/// <summary>
///     Pins the conservative contract of <see cref="GameEngine{TInfocomGame,TContext}.NormalizeLookAt" />
///     (issues #312 / #283). The behavioral <c>ExamineVerbsTests</c> in each game cover the positive
///     "look at X routes to examine" path end-to-end; these unit tests lock the <em>negative</em> cases
///     the integration tests leave implicit — bare "look", "look around" and other "look &lt;preposition&gt;"
///     phrases must NOT be rewritten — so a future broadening of the prefix match is caught immediately.
/// </summary>
public class NormalizeLookAtTests
{
    // The static helper is generic-class-scoped; any closed generic exposes the same implementation.
    private static string? Normalize(string? input) =>
        GameEngine<ZorkI, ZorkIContext>.NormalizeLookAt(input);

    [TestCase("look at mailbox", "examine mailbox")]
    [TestCase("look at door", "examine door")]
    [TestCase("look at jade figurine", "examine jade figurine")]
    [TestCase("look at the lamp", "examine the lamp")]
    [TestCase("LOOK AT MAILBOX", "examine MAILBOX")] // case-insensitive prefix, noun case preserved
    [TestCase("  look at mailbox", "examine mailbox")] // leading whitespace tolerated
    [TestCase("look at   mailbox", "examine mailbox")] // collapses padding after the prefix
    public void Rewrites_LookAtNoun_ToExamine(string input, string expected)
    {
        Normalize(input).Should().Be(expected);
    }

    [TestCase("look")] // bare room-description command
    [TestCase("look around")]
    [TestCase("l")]
    [TestCase("look under the rug")] // distinct "look <preposition>" interactions
    [TestCase("look in the box")]
    [TestCase("look through the crack")]
    [TestCase("look behind the painting")]
    [TestCase("examine mailbox")] // already canonical — untouched
    [TestCase("take lamp")]
    [TestCase("quickly look at the lamp")] // only a leading prefix is rewritten, not a mid-sentence one
    public void LeavesUnrelatedInput_Untouched(string input)
    {
        Normalize(input).Should().Be(input);
    }

    [TestCase("look at")] // verb phrase with no noun — nothing to examine, leave alone
    [TestCase("look at ")]
    [TestCase("look at   ")]
    public void LeavesNounlessLookAt_Untouched(string input)
    {
        Normalize(input).Should().Be(input);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void HandlesNullOrBlank_Untouched(string? input)
    {
        Normalize(input).Should().Be(input);
    }
}
