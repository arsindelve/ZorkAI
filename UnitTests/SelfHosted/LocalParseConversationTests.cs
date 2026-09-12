using ChatLambda;
using FluentAssertions;

namespace UnitTests.SelfHosted;

/// <summary>
///     Tests for the deterministic half of the local conversation classifier (issue #383): pulling
///     a classification out of whatever a local model produced. The model call itself is
///     nondeterministic and not unit-tested; on failure the classifier returns (false, "") and the
///     engine's direct-address fast paths take over.
/// </summary>
public class LocalParseConversationTests
{
    [Test]
    public void Should_ParseCleanClassification()
    {
        var parsed = LocalParseConversation.TryExtractClassification(
            "{\"is_conversational\": true, \"rewritten\": \"go north\"}",
            out var isConversational, out var rewritten);

        parsed.Should().BeTrue();
        isConversational.Should().BeTrue();
        rewritten.Should().Be("go north");
    }

    [Test]
    public void Should_ParseNonConversationalClassification()
    {
        var parsed = LocalParseConversation.TryExtractClassification(
            "{\"is_conversational\": false, \"rewritten\": \"\"}",
            out var isConversational, out var rewritten);

        parsed.Should().BeTrue();
        isConversational.Should().BeFalse();
        rewritten.Should().BeEmpty();
    }

    [Test]
    public void Should_TolerateCodeFencesAndProse()
    {
        var raw = "Here is my analysis:\n```json\n{\"is_conversational\": true, \"rewritten\": \"wait here\"}\n```";

        var parsed = LocalParseConversation.TryExtractClassification(raw, out var isConversational, out var rewritten);

        parsed.Should().BeTrue();
        isConversational.Should().BeTrue();
        rewritten.Should().Be("wait here");
    }

    [Test]
    public void Should_AcceptAlternatePropertyNames()
    {
        var parsed = LocalParseConversation.TryExtractClassification(
            "{\"isConversational\": true, \"response\": \"what happened?\"}",
            out var isConversational, out var rewritten);

        parsed.Should().BeTrue();
        isConversational.Should().BeTrue();
        rewritten.Should().Be("what happened?");
    }

    [Test]
    public void Should_Fail_When_ClassificationBoolIsMissing()
    {
        var parsed = LocalParseConversation.TryExtractClassification(
            "{\"rewritten\": \"go north\"}", out _, out _);

        parsed.Should().BeFalse();
    }

    [TestCase("total garbage")]
    [TestCase("{broken json")]
    [TestCase("")]
    [TestCase(null)]
    public void Should_Fail_When_OutputIsUnusable(string? raw)
    {
        LocalParseConversation.TryExtractClassification(raw, out var isConversational, out _).Should().BeFalse();
        isConversational.Should().BeFalse();
    }
}
