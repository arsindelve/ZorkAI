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
}
