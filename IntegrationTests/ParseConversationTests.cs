using ChatLambda;
using Microsoft.Extensions.Logging;
using Moq;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Parallelizable(ParallelScope.Children)]
public class ParseConversationTests
{
    [Test]
    public async Task ParseConversation_ReturnsNo_WhenNotCommunication()
    {
        var target = new ParseConversation(null, Mock.Of<ILogger<ParseConversation>>());
        var result = await target.ParseAsync("kick Floyd");
        Console.WriteLine($"Input: kick Floyd");
        Console.WriteLine($"Is Conversational: {result.isConversational}");
        Console.WriteLine($"Response: '{result.response}'");

        Assert.That(result.isConversational, Is.False, "Physical action should not be conversational");
        Assert.That(result.response, Is.Empty, "Response should be empty when not conversational");
    }

    [Test]
    public async Task ParseConversation_ReturnsRewrittenMessage_WhenCommunication()
    {
        var target = new ParseConversation(null, Mock.Of<ILogger<ParseConversation>>());
        var result = await target.ParseAsync("tell Floyd I love him");
        Console.WriteLine($"Input: tell Floyd I love him");
        Console.WriteLine($"Is Conversational: {result.isConversational}");
        Console.WriteLine($"Response: '{result.response}'");

        Assert.That(result.isConversational, Is.True, "Communication should be conversational");
        Assert.That(result.response, Is.Not.Empty, "Response should contain rewritten message");
    }

    [Test]
    public async Task ParseConversation_ReturnsRewrittenMessage_WhenDirectSpeech()
    {
        var target = new ParseConversation(null, Mock.Of<ILogger<ParseConversation>>());
        var result = await target.ParseAsync("Floyd, you're such a good friend!");
        Console.WriteLine($"Input: Floyd, you're such a good friend!");
        Console.WriteLine($"Is Conversational: {result.isConversational}");
        Console.WriteLine($"Response: '{result.response}'");

        Assert.That(result.isConversational, Is.True, "Direct speech should be conversational");
        Assert.That(result.response, Is.Not.Empty, "Response should contain rewritten message");
    }

    [Test]
    public async Task ParseConversation_ReturnsNo_WhenInternalState()
    {
        var target = new ParseConversation(null, Mock.Of<ILogger<ParseConversation>>());
        var result = await target.ParseAsync("I'm bored");
        Console.WriteLine($"Input: I'm bored");
        Console.WriteLine($"Is Conversational: {result.isConversational}");
        Console.WriteLine($"Response: '{result.response}'");

        Assert.That(result.isConversational, Is.False, "Internal state should not be conversational");
        Assert.That(result.response, Is.Empty, "Response should be empty when not conversational");
    }

    [Test]
    public async Task ParseConversation_ReturnsRewrittenMessage_WhenYelling()
    {
        var target = new ParseConversation(null, Mock.Of<ILogger<ParseConversation>>());
        var result = await target.ParseAsync("yell at Floyd to go slowly and be careful");
        Console.WriteLine($"Input: yell at Floyd to go slowly and be careful");
        Console.WriteLine($"Is Conversational: {result.isConversational}");
        Console.WriteLine($"Response: '{result.response}'");

        Assert.That(result.isConversational, Is.True, "Yelling instruction should be conversational");
        Assert.That(result.response, Is.Not.Empty, "Response should contain rewritten message");
    }

    [Test]
    public async Task ParseConversation_ReturnsRewrittenMessage_WhenAskingAboutTopic()
    {
        var target = new ParseConversation(null, Mock.Of<ILogger<ParseConversation>>());
        var result = await target.ParseAsync("ask ambassador about his home planet");
        Console.WriteLine($"Input: ask ambassador about his home planet");
        Console.WriteLine($"Is Conversational: {result.isConversational}");
        Console.WriteLine($"Response: '{result.response}'");

        Assert.That(result.isConversational, Is.True, "'ask [character] about [topic]' should be conversational");
        Assert.That(result.response, Is.Not.Empty, "Response should contain rewritten message about the topic");
        Assert.That(result.response.ToLowerInvariant(), Does.Contain("about"),
            "Response should preserve the 'about [topic]' pattern");
    }
}