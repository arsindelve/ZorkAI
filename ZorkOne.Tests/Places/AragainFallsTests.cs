using FluentAssertions;
using GameEngine;
using Model.Intent;
using Model.Interface;
using Moq;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class AragainFallsTests : EngineTestsBase
{
    [Test]
    public async Task LookUnderRainbow_AtEndOfRainbow_DescribesRiver()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<EndOfRainbow>();

        var response = await target.GetResponse("look under rainbow");

        response.Should().Contain("The Frigid River flows under the rainbow.");
    }

    [Test]
    public async Task LookUnderRainbow_DescribesRiver()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<AragainFalls>();

        var response = await target.GetResponse("look under rainbow");

        response.Should().Contain("The Frigid River flows under the rainbow.");
    }

    [Test]
    public async Task LookUnderRainbow_WithProductionParserShape_DescribesRiver()
    {
        var parser = ParserReturning(new SimpleIntent
        {
            Verb = "look",
            Noun = "rainbow",
            OriginalInput = "look under rainbow"
        });
        var target = GetTarget(parser.Object);
        target.Context.CurrentLocation = Repository.GetLocation<AragainFalls>();

        var response = await target.GetResponse("look under rainbow");

        response.Should().Contain("The Frigid River flows under the rainbow.");
    }

    [Test]
    public async Task ThroughRainbow_WhenRainbowIsNotSolid_Refuses()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<AragainFalls>();

        var response = await target.GetResponse("through rainbow");

        response.Should().Contain("Can you walk on water vapor?");
    }

    [Test]
    public async Task GoThroughRainbow_WithProductionParserShape_Refuses()
    {
        var parser = ParserReturning(new SimpleIntent
        {
            Verb = "go",
            Noun = "rainbow",
            OriginalInput = "go through rainbow"
        });
        var target = GetTarget(parser.Object);
        target.Context.CurrentLocation = Repository.GetLocation<AragainFalls>();

        var response = await target.GetResponse("go through rainbow");

        response.Should().Contain("Can you walk on water vapor?");
    }

    [Test]
    public async Task GoThroughRainbow_WhenRainbowIsNotSolid_Refuses()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<AragainFalls>();

        var response = await target.GetResponse("go through rainbow");

        response.Should().Contain("Can you walk on water vapor?");
    }

    [Test]
    public async Task ThroughRainbow_AtEndOfRainbow_WhenRainbowIsNotSolid_Refuses()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<EndOfRainbow>();

        var response = await target.GetResponse("through rainbow");

        response.Should().Contain("Can you walk on water vapor?");
    }

    [Test]
    public async Task CrossRainbow_AtEndOfRainbow_WhenRainbowIsSolid_MovesToAragainFalls()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<EndOfRainbow>();
        Repository.GetLocation<EndOfRainbow>().RainbowIsSolid = true;

        var response = await target.GetResponse("cross rainbow");

        response.Should().Contain("Aragain Falls");
        target.Context.CurrentLocation.Should().BeOfType<AragainFalls>();
    }

    [Test]
    public async Task CrossRainbow_OnTheRainbow_WhenRainbowIsSolid_AsksForDirection()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<OnTheRainbow>();
        Repository.GetLocation<EndOfRainbow>().RainbowIsSolid = true;

        var response = await target.GetResponse("cross rainbow");

        response.Should().Contain("You'll have to say which way...");
        target.Context.CurrentLocation.Should().BeOfType<OnTheRainbow>();
    }

    [Test]
    public async Task CrossRainbow_WhenRainbowIsNotSolid_Refuses()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<AragainFalls>();

        var response = await target.GetResponse("cross rainbow");

        response.Should().Contain("Can you walk on water vapor?");
    }

    private static Mock<IIntentParser> ParserReturning(IntentBase intent)
    {
        var parser = new Mock<IIntentParser>();
        parser.Setup(p => p.DetermineSystemIntentType(It.IsAny<string>())).Returns((IntentBase?)null);
        parser.Setup(p => p.DetermineGlobalIntentType(It.IsAny<string>())).Returns((IntentBase?)null);
        parser.Setup(p => p.ResolvePronounsAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync((string?)null);
        parser.Setup(p => p.DetermineComplexIntentType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(intent);
        return parser;
    }
}
