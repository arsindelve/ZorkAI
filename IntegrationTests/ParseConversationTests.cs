using ChatLambda;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Parallelizable(ParallelScope.Children)]
public class ParseConversationTests
{
    [Test]
    public async Task ParseConversation_ReturnsNo_WhenNotCommunication()
    {
        var target = new ParseConversation(null);
        var result = await target.ParseAsync("kick Floyd");
        Console.WriteLine($"Input: kick Floyd");
        Console.WriteLine($"Is No: {result.isNo}");
        Console.WriteLine($"Response: '{result.response}'");
        
        Assert.That(result.isNo, Is.True, "Physical action should return 'No'");
        Assert.That(result.response, Is.Empty, "Response should be empty when 'No'");
    }

    [Test]
    public async Task ParseConversation_ReturnsRewrittenMessage_WhenCommunication()
    {
        var target = new ParseConversation(null);
        var result = await target.ParseAsync("tell Floyd I love him");
        Console.WriteLine($"Input: tell Floyd I love him");
        Console.WriteLine($"Is No: {result.isNo}");
        Console.WriteLine($"Response: '{result.response}'");
        
        Assert.That(result.isNo, Is.False, "Communication should not return 'No'");
        Assert.That(result.response, Is.Not.Empty, "Response should contain rewritten message");
    }

    [Test]
    public async Task ParseConversation_ReturnsRewrittenMessage_WhenDirectSpeech()
    {
        var target = new ParseConversation(null);
        var result = await target.ParseAsync("Floyd, you're such a good friend!");
        Console.WriteLine($"Input: Floyd, you're such a good friend!");
        Console.WriteLine($"Is No: {result.isNo}");
        Console.WriteLine($"Response: '{result.response}'");
        
        Assert.That(result.isNo, Is.False, "Direct speech should not return 'No'");
        Assert.That(result.response, Is.Not.Empty, "Response should contain rewritten message");
    }

    [Test]
    public async Task ParseConversation_ReturnsNo_WhenInternalState()
    {
        var target = new ParseConversation(null);
        var result = await target.ParseAsync("I'm bored");
        Console.WriteLine($"Input: I'm bored");
        Console.WriteLine($"Is No: {result.isNo}");
        Console.WriteLine($"Response: '{result.response}'");
        
        Assert.That(result.isNo, Is.True, "Internal state should return 'No'");
        Assert.That(result.response, Is.Empty, "Response should be empty when 'No'");
    }

    [Test]
    public async Task ParseConversation_ReturnsRewrittenMessage_WhenYelling()
    {
        var target = new ParseConversation(null);
        var result = await target.ParseAsync("yell at Floyd to go slowly and be careful");
        Console.WriteLine($"Input: yell at Floyd to go slowly and be careful");
        Console.WriteLine($"Is No: {result.isNo}");
        Console.WriteLine($"Response: '{result.response}'");
        
        Assert.That(result.isNo, Is.False, "Yelling instruction should not return 'No'");
        Assert.That(result.response, Is.Not.Empty, "Response should contain rewritten message");
    }
}