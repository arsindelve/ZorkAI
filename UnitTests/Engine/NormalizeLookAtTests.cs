using FluentAssertions;
using GameEngine;
using ZorkOne;

namespace UnitTests.Engine;

/// <summary>
///     Pins the contract of <see cref="GameEngine{TInfocomGame,TContext}.NormalizeLookAt" />
///     (issues #312 / #283 for "look at"; issue #396 for "look in"/"look inside"). The behavioral
///     <c>ExamineVerbsTests</c> in each game cover the positive "look at X routes to examine" path
///     end-to-end; these unit tests lock the boundary cases the integration tests leave implicit —
///     which "look &lt;preposition&gt;" phrases route to examine ("look in"/"look inside", the
///     container-inspection phrasings that mirror the original ZIL's LOOK IN -> V-LOOK-INSIDE) and
///     which stay untouched (bare "look"/"look around", "look under"/"look behind"/"look through",
///     and the "look in inventory" phrasing owned elsewhere) — so a future change to the prefix
///     matching is caught immediately.
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

    // Issue #396: "look in <container>" / "look inside <container>" — the canonical Infocom phrasing
    // for inspecting an open container's contents — must route to examine, not be handed to the AI
    // parser (which mis-tags the "in" as a movement, yielding "You cannot go that way.").
    [TestCase("look in the box", "examine the box")]
    [TestCase("look inside the box", "examine the box")]
    [TestCase("look in panel", "examine panel")]
    [TestCase("look inside panel", "examine panel")]
    [TestCase("LOOK IN MAILBOX", "examine MAILBOX")] // case-insensitive prefix, noun case preserved
    [TestCase("  look in mailbox", "examine mailbox")] // leading whitespace tolerated
    [TestCase("look inside   sack", "examine sack")] // collapses padding after the prefix
    public void Rewrites_LookInNoun_ToExamine(string input, string expected)
    {
        Normalize(input).Should().Be(expected);
    }

    // The "look in inventory" / "look in my inventory" phrasing is owned by GlobalCommandFactory's
    // InventoryProcessor, which runs AFTER NormalizeLookAt on the literal text — rewriting it to
    // "examine inventory" would break the inventory listing (issue #396). Must be left untouched.
    [TestCase("look in inventory")]
    [TestCase("look in my inventory")]
    [TestCase("LOOK IN INVENTORY")]
    public void PreservesLookInInventory_ForInventoryProcessor(string input)
    {
        Normalize(input).Should().Be(input);
    }

    [TestCase("look")] // bare room-description command
    [TestCase("look around")]
    [TestCase("l")]
    [TestCase("look under the rug")] // distinct "look <preposition>" interactions, left alone
    [TestCase("look through the crack")]
    [TestCase("look behind the painting")]
    // "look into" is deliberately NOT rewritten: it is not an original ZIL syntax, it carries an
    // "investigate" reading, and the existing "look into relay" Planetfall path depends on it being
    // left alone. Issue #396 marks it explicitly optional.
    [TestCase("look into the box")]
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
    [TestCase("look in")]
    [TestCase("look in ")]
    [TestCase("look inside")]
    [TestCase("look inside ")]
    public void LeavesNounlessLookPhrase_Untouched(string input)
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
