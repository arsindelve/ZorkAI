using Bedrock;
using Model.Intent;
using Model.Movement;

namespace UnitTests.Parser;

public class ClaudeParserTests
{
    private Mock<IClaudeFourParserClient> _client = null!;

    private ClaudeFourParser GetTarget()
    {
        _client = new Mock<IClaudeFourParserClient>();
        return new ClaudeFourParser(_client.Object);
    }

    [Test]
    public async Task SimpleDirection()
    {
        var target = GetTarget();
        
        _client.Setup(c =>
            c.GetResponse("b", "a")).ReturnsAsync(
            "<intent>move</intent>\n<verb>follow</verb>\n<noun>crawlway</noun>\n<direction>south</direction>");

        IntentBase response = await target.AskTheAIParser("a", "b", "c");

        response.Should().BeAssignableTo<MoveIntent>();
        response.As<MoveIntent>().Direction.Should().Be(Direction.S);
    }
    
    [Test]
    public async Task SimpleAction()
    {
        var target = GetTarget();
        
        _client.Setup(c =>
            c.GetResponse("b", "a")).ReturnsAsync(
            "<intent>act</intent>\n<verb>open</verb>\n<noun>mailbox</noun>");

        IntentBase response = await target.AskTheAIParser("a", "b", "c");

        response.Should().BeAssignableTo<SimpleIntent>();
        response.As<SimpleIntent>().Verb.Should().Be("open");
        response.As<SimpleIntent>().Noun.Should().Be("mailbox");
    }
    
    [Test]
    public async Task BogusDirection()
    {
        var target = GetTarget();
        
        _client.Setup(c =>
            c.GetResponse("b", "a")).ReturnsAsync(
            "<intent>move</intent>\n<verb>follow</verb>\n<noun>crawlway</noun>\n<direction>bogus</direction>");

        IntentBase response = await target.AskTheAIParser("a", "b", "c");

        response.Should().BeOfType<NullIntent>();
    }
    
    [Test]
    public async Task NoDirection()
    {
        var target = GetTarget();
        
        _client.Setup(c =>
            c.GetResponse("b", "a")).ReturnsAsync(
            "<intent>move</intent>\n<verb>follow</verb>\n<noun>crawlway</noun>");

        IntentBase response = await target.AskTheAIParser("a", "b", "c");

        response.Should().BeOfType<NullIntent>();
    }
    
    [Test]
    public async Task NoAIntent()
    {
        var target = GetTarget();
        
        _client.Setup(c =>
            c.GetResponse("b", "a")).ReturnsAsync(
            "<verb>follow</verb>\n<noun>crawlway</noun>");

        IntentBase response = await target.AskTheAIParser("a", "b", "c");

        response.Should().BeOfType<NullIntent>();
    }
    
    [Test]
    public async Task SimpleAction_NoVerb()
    {
        var target = GetTarget();
        
        _client.Setup(c =>
            c.GetResponse("b", "a")).ReturnsAsync(
            "<intent>act</intent>\n<noun>mailbox</noun>");

        IntentBase response = await target.AskTheAIParser("a", "b", "c");

        response.Should().BeOfType<NullIntent>();
    }
    
    [Test]
    public async Task SimpleAction_NoNoun()
    {
        var target = GetTarget();
        
        _client.Setup(c =>
            c.GetResponse("b", "a")).ReturnsAsync(
            "<intent>act</intent>\n<verb>open</verb>");

        IntentBase response = await target.AskTheAIParser("a", "b", "c");

        response.Should().BeOfType<NullIntent>();
    }

}