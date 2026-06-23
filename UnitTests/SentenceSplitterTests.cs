using GameEngine;

namespace UnitTests;

[TestFixture]
public class SentenceSplitterTests
{
    [Test]
    public void Split_SingleSentence_ReturnsOneItem()
    {
        var result = SentenceSplitter.Split("take lamp");

        result.Should().HaveCount(1);
        result[0].Should().Be("take lamp");
    }

    [Test]
    public void Split_MultipleSentences_ReturnsCorrectCount()
    {
        var result = SentenceSplitter.Split("take lamp. go north. look");

        result.Should().HaveCount(3);
        result[0].Should().Be("take lamp");
        result[1].Should().Be("go north");
        result[2].Should().Be("look");
    }

    [Test]
    public void Split_TwoSentences_ReturnsCorrectItems()
    {
        var result = SentenceSplitter.Split("inventory. score");

        result.Should().HaveCount(2);
        result[0].Should().Be("inventory");
        result[1].Should().Be("score");
    }

    [Test]
    public void Split_WithAbbreviation_Mr_DoesNotSplit()
    {
        var result = SentenceSplitter.Split("talk to Mr. Jones");

        result.Should().HaveCount(1);
        result[0].Should().Be("talk to Mr. Jones");
    }

    [Test]
    public void Split_WithAbbreviation_Dr_DoesNotSplit()
    {
        var result = SentenceSplitter.Split("ask Dr. Smith about the key");

        result.Should().HaveCount(1);
        result[0].Should().Be("ask Dr. Smith about the key");
    }

    [Test]
    public void Split_WithAbbreviation_Mrs_DoesNotSplit()
    {
        var result = SentenceSplitter.Split("give flower to Mrs. Johnson");

        result.Should().HaveCount(1);
        result[0].Should().Be("give flower to Mrs. Johnson");
    }

    [Test]
    public void Split_CardinalDirection_N_KeepsAsIs()
    {
        var result = SentenceSplitter.Split("go N.");

        result.Should().HaveCount(1);
        result[0].Should().Be("go N.");
    }

    [Test]
    public void Split_EmptyString_ReturnsEmptyList()
    {
        var result = SentenceSplitter.Split("");

        result.Should().BeEmpty();
    }

    [Test]
    public void Split_NullString_ReturnsEmptyList()
    {
        var result = SentenceSplitter.Split(null);

        result.Should().BeEmpty();
    }

    [Test]
    public void Split_WhitespaceOnly_ReturnsEmptyList()
    {
        var result = SentenceSplitter.Split("   ");

        result.Should().BeEmpty();
    }

    [Test]
    public void Split_TrailingPeriod_RemovesPeriod()
    {
        var result = SentenceSplitter.Split("take lamp.");

        result.Should().HaveCount(1);
        result[0].Should().Be("take lamp");
    }

    [Test]
    public void Split_MultiplePeriodsInRow_HandlesGracefully()
    {
        var result = SentenceSplitter.Split("take lamp.. go north");

        result.Should().HaveCount(2);
        result[0].Should().Be("take lamp");
        result[1].Should().Be("go north");
    }

    [Test]
    public void Split_ExtraSpaces_HandlesGracefully()
    {
        var result = SentenceSplitter.Split("take lamp.  go north.   look");

        result.Should().HaveCount(3);
        result[0].Should().Be("take lamp");
        result[1].Should().Be("go north");
        result[2].Should().Be("look");
    }

    [Test]
    public void Split_ComplexMultiCommand_ReturnsCorrectItems()
    {
        var result = SentenceSplitter.Split("open mailbox. take leaflet. read leaflet. drop leaflet");

        result.Should().HaveCount(4);
        result[0].Should().Be("open mailbox");
        result[1].Should().Be("take leaflet");
        result[2].Should().Be("read leaflet");
        result[3].Should().Be("drop leaflet");
    }

    [Test]
    public void Split_WithEtcAbbreviation_DoesNotSplit()
    {
        var result = SentenceSplitter.Split("take keys, coins, etc. and go north");

        result.Should().HaveCount(1);
        result[0].Should().Be("take keys, coins, etc. and go north");
    }

    [Test]
    public void Split_MixedCommands_ReturnsCorrectItems()
    {
        var result = SentenceSplitter.Split("n. take sword. e. kill troll with sword");

        result.Should().HaveCount(4);
        result[0].Should().Be("n.");
        result[1].Should().Be("take sword");
        result[2].Should().Be("e.");
        result[3].Should().Be("kill troll with sword");
    }

    [Test]
    public void Split_OnlyPeriods_ReturnsEmptyList()
    {
        var result = SentenceSplitter.Split("...");

        result.Should().BeEmpty();
    }

    [Test]
    public void Split_RealWorldExample1_ZorkCommands()
    {
        var result = SentenceSplitter.Split("open window. enter house. take lamp");

        result.Should().HaveCount(3);
        result[0].Should().Be("open window");
        result[1].Should().Be("enter house");
        result[2].Should().Be("take lamp");
    }

    [Test]
    public void Split_RealWorldExample2_Navigation()
    {
        var result = SentenceSplitter.Split("south. west. open door. north");

        result.Should().HaveCount(4);
        result[0].Should().Be("south");
        result[1].Should().Be("west");
        result[2].Should().Be("open door");
        result[3].Should().Be("north");
    }

    [Test]
    public void Split_SentenceWithoutPeriod_ReturnsOriginal()
    {
        var result = SentenceSplitter.Split("take everything");

        result.Should().HaveCount(1);
        result[0].Should().Be("take everything");
    }

    [Test]
    public void Split_PeriodAtStartAndEnd_HandlesGracefully()
    {
        var result = SentenceSplitter.Split(".take lamp. go north.");

        result.Should().HaveCount(2);
        result[0].Should().Be("take lamp");
        result[1].Should().Be("go north");
    }

    [Test]
    public void Split_SingleLetterDirections_SplitsCorrectly()
    {
        var result = SentenceSplitter.Split("e. w.");

        result.Should().HaveCount(2);
        result[0].Should().Be("e.");
        result[1].Should().Be("w.");
    }

    [Test]
    public void Split_MultipleCardinalDirections_EachIsSeparate()
    {
        var result = SentenceSplitter.Split("n. s. e. w.");

        result.Should().HaveCount(4);
        result[0].Should().Be("n.");
        result[1].Should().Be("s.");
        result[2].Should().Be("e.");
        result[3].Should().Be("w.");
    }

    [Test]
    public void Split_MixedDirectionsAndCommands_SplitsCorrectly()
    {
        var result = SentenceSplitter.Split("e. take sword. w.");

        result.Should().HaveCount(3);
        result[0].Should().Be("e.");
        result[1].Should().Be("take sword");
        result[2].Should().Be("w.");
    }

    [Test]
    public void Split_NoSpacesBetweenCommands_SplitsCorrectly()
    {
        var result = SentenceSplitter.Split("look.wait.wait");

        result.Should().HaveCount(3);
        result[0].Should().Be("look");
        result[1].Should().Be("wait");
        result[2].Should().Be("wait");
    }

    [Test]
    public void Split_NoSpacesBetweenDirections_SplitsCorrectly()
    {
        var result = SentenceSplitter.Split("e.w.n.s");

        result.Should().HaveCount(4);
        result[0].Should().Be("e.");
        result[1].Should().Be("w.");
        result[2].Should().Be("n.");
        result[3].Should().Be("s.");
    }

    [Test]
    public void Split_MixedSpacedAndNonSpaced_SplitsCorrectly()
    {
        var result = SentenceSplitter.Split("take lamp.turn it on. go north");

        result.Should().HaveCount(3);
        result[0].Should().Be("take lamp");
        result[1].Should().Be("turn it on");
        result[2].Should().Be("go north");
    }

    [Test]
    public void Split_NoAnswerFollowedByCommand_SplitsCorrectly()
    {
        // "no" is a standalone answer (e.g. to a yes/no prompt), not an abbreviation,
        // so it must split rather than merge with the following command.
        var result = SentenceSplitter.Split("no. take lamp");

        result.Should().HaveCount(2);
        result[0].Should().Be("no");
        result[1].Should().Be("take lamp");
    }

    // --- #292: a period INSIDE quoted speech is dialogue punctuation, not a command delimiter ----
    // Quoted speech ("…") routes to the present NPC via ConversationHandler.TryStripQuotedSpeech,
    // but that runs AFTER this splitter. If the splitter breaks "hello. I love you" on the inner
    // period, only the first fragment keeps its opening quote and reaches the NPC; the rest leaks to
    // the third-person narrator (re-opening the #284 bug). So periods inside quotes must not split.

    [Test]
    public void Split_PeriodInsideQuotes_DoesNotSplit()
    {
        var result = SentenceSplitter.Split("\"hello. I love you\"");

        result.Should().HaveCount(1);
        result[0].Should().Be("\"hello. I love you\"");
    }

    [Test]
    public void Split_MultiplePeriodsInsideQuotes_DoesNotSplit()
    {
        var result = SentenceSplitter.Split("\"stop. drop. roll.\"");

        result.Should().HaveCount(1);
        result[0].Should().Be("\"stop. drop. roll.\"");
    }

    [Test]
    public void Split_UnterminatedQuote_DoesNotSplitOnInnerPeriod()
    {
        // TryStripQuotedSpeech accepts an unterminated quote, so the splitter must keep it whole too.
        var result = SentenceSplitter.Split("\"hello. I love you");

        result.Should().HaveCount(1);
        result[0].Should().Be("\"hello. I love you");
    }

    [Test]
    public void Split_PeriodInsideSmartQuotes_DoesNotSplit()
    {
        var result = SentenceSplitter.Split("“hello. I love you”");

        result.Should().HaveCount(1);
        result[0].Should().Be("“hello. I love you”");
    }

    [Test]
    public void Split_QuotedClauseThenChainedCommand_SplitsAtOutsidePeriod()
    {
        // The period inside the quotes is protected; the period after the closing quote still splits.
        var result = SentenceSplitter.Split("say \"hello. there\". north");

        result.Should().HaveCount(2);
        result[0].Should().Be("say \"hello. there\"");
        result[1].Should().Be("north");
    }

    [Test]
    public void Split_CommandThenQuotedSpeech_SplitsAtOutsidePeriodOnly()
    {
        var result = SentenceSplitter.Split("east. \"hello. I love you\"");

        result.Should().HaveCount(2);
        result[0].Should().Be("east");
        result[1].Should().Be("\"hello. I love you\"");
    }

    [Test]
    public void Split_MixedStraightAndSmartQuotePair_DoesNotSplit()
    {
        // The toggle treats any double quote as open-or-close, so a mismatched pair (straight open,
        // smart close) still brackets the speech and protects the inner period.
        var result = SentenceSplitter.Split("\"hello. I love you”");

        result.Should().HaveCount(1);
        result[0].Should().Be("\"hello. I love you”");
    }
}
