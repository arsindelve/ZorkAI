using ZorkConsole;

namespace Console.Tests;

/// <summary>
///     Parsing for the console's self-hosting flags (issue #383). Goes through the pure
///     <see cref="SelfHostFlags.Parse" /> so nothing here mutates the process environment.
/// </summary>
public class SelfHostFlagsTests
{
    [Test]
    public void Should_ReturnNothing_When_OnlyTheGameIsGiven()
    {
        SelfHostFlags.Parse(["ZorkOne"]).Should().BeEmpty();
    }

    [Test]
    public void Should_ReturnNothing_When_ArgsAreNull()
    {
        SelfHostFlags.Parse(null).Should().BeEmpty();
    }

    [Test]
    public void Should_MapEveryFlag_ToItsEnvironmentVariable()
    {
        var parsed = SelfHostFlags.Parse([
            "ZorkOne", "--provider", "ollama", "--model", "llama3.1:8b", "--endpoint", "http://gpu-box:8080/v1"
        ]);

        parsed.Should().HaveCount(3);
        parsed["ZORKAI_PROVIDER"].Should().Be("ollama");
        parsed["OPENAI_MODEL"].Should().Be("llama3.1:8b");
        parsed["OPENAI_BASE_URL"].Should().Be("http://gpu-box:8080/v1");
    }

    [Test]
    public void Should_NotRereadAConsumedValue_AsAFlag()
    {
        // The regression: the loop advanced by one, so the value it had just consumed was inspected
        // again on the next pass. "--model --provider ollama" therefore set OPENAI_MODEL to
        // "--provider" AND ZORKAI_PROVIDER to "ollama" — two wrong answers from one typo.
        var parsed = SelfHostFlags.Parse(["ZorkOne", "--model", "--provider", "ollama"]);

        parsed.Should().HaveCount(1);
        parsed["OPENAI_MODEL"].Should().Be("--provider");
        parsed.Should().NotContainKey("ZORKAI_PROVIDER");
    }

    [Test]
    public void Should_IgnoreTheGameName_EvenWhenItLooksLikeAFlag()
    {
        SelfHostFlags.Parse(["--provider", "ollama"]).Should().BeEmpty();
    }

    [Test]
    public void Should_IgnoreATrailingFlag_WithNoValue()
    {
        SelfHostFlags.Parse(["ZorkOne", "--provider"]).Should().BeEmpty();
    }

    [Test]
    public void Should_IgnoreUnknownFlags()
    {
        var parsed = SelfHostFlags.Parse(["ZorkOne", "--verbose", "yes", "--model", "m"]);

        parsed.Should().HaveCount(1);
        parsed["OPENAI_MODEL"].Should().Be("m");
    }

    [Test]
    public void Should_LetTheLastOccurrenceWin_When_AFlagIsRepeated()
    {
        var parsed = SelfHostFlags.Parse(["ZorkOne", "--model", "first", "--model", "second"]);

        parsed["OPENAI_MODEL"].Should().Be("second");
    }

    [TestCase("--PROVIDER")]
    [TestCase("--Provider")]
    public void Should_MatchFlagsCaseInsensitively(string flag)
    {
        SelfHostFlags.Parse(["ZorkOne", flag, "ollama"])["ZORKAI_PROVIDER"].Should().Be("ollama");
    }
}
