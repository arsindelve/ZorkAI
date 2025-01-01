using FluentAssertions;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

public class FloodingTests : EngineTestsBase
{
    [Test]
    public async Task DayOne()
    {
        var engine = GetTarget();
        engine.Context.Day = 1;
        StartHere<Courtyard>();

        // Couryard
        var response = await engine.GetResponse("look");
        response.Should().NotContain("From the direction of the stairway comes the sound of ocean surf");
        
        // Winding Stair
        response = await engine.GetResponse("down");
        response.Should().NotContain("You hear the lapping of water from below");
        
        // Balcony
        response = await engine.GetResponse("down");
        response.Should().NotContain("You hear the lapping of water from below");
        
        // Crag
        response = await engine.GetResponse("down");
        response.Should().Contain("Crag");
    }
    
    [Test]
    public async Task DayTwo()
    {
        var engine = GetTarget();
        engine.Context.Day = 2;
        StartHere<Courtyard>();

        // Couryard
        var response = await engine.GetResponse("look");
        response.Should().NotContain("From the direction of the stairway comes the sound of ocean surf");
        
        // Winding Stair
        response = await engine.GetResponse("down");
        response.Should().NotContain("You hear the lapping of water from below");
        
        // Balcony
        response = await engine.GetResponse("down");
        response.Should().Contain("The ocean waters swirl below. The crag where you landed yesterday is now underwater");
        
        // Crag
        response = await engine.GetResponse("down");
        response.Should().Contain("Underwater");
    }
    
    [Test]
    public async Task DayThree()
    {
        var engine = GetTarget();
        engine.Context.Day = 3;
        StartHere<Courtyard>();

        // Couryard
        var response = await engine.GetResponse("look");
        response.Should().NotContain("From the direction of the stairway comes the sound of ocean surf");
        
        // Winding Stair
        response = await engine.GetResponse("down");
        response.Should().NotContain("You hear the lapping of water from below");
        
        // Balcony
        response = await engine.GetResponse("down");
        response.Should().Contain("Ocean waters are lapping at the base of the balcony");
        response.Should().NotContain("The crag where you landed yesterday is now underwater");

        // Crag
        response = await engine.GetResponse("down");
        response.Should().Contain("Underwater");
    }
    
    [Test]
    public async Task DayFour()
    {
        var engine = GetTarget();
        engine.Context.Day = 4;
        StartHere<Courtyard>();

        // Couryard
        var response = await engine.GetResponse("look");
        response.Should().NotContain("From the direction of the stairway comes the sound of ocean surf");
        
        // Winding Stair
        response = await engine.GetResponse("down");
        response.Should().Contain("You hear the lapping of water from below");
        
        // Balcony
        response = await engine.GetResponse("down");
        response.Should().Contain("Underwater");
    }
    
    [Test]
    public async Task DayFive()
    {
        var engine = GetTarget();
        engine.Context.Day = 5;
        StartHere<Courtyard>();

        // Couryard
        var response = await engine.GetResponse("look");
        response.Should().Contain("From the direction of the stairway comes the sound of ocean surf");
        
        // Winding Stair
        response = await engine.GetResponse("down");
        response.Should().Contain("You can see ocean water splashing against the steps below you");
        
        // Balcony
        response = await engine.GetResponse("down");
        response.Should().Contain("Underwater");
    }
    
    [Test]
    public async Task DaySix()
    {
        var engine = GetTarget();
        engine.Context.Day = 6;
        StartHere<Courtyard>();

        // Couryard
        var response = await engine.GetResponse("look");
        response.Should().Contain("From the direction of the stairway comes the sound of ocean surf");
        
        // Winding Stair
        response = await engine.GetResponse("down");
        response.Should().Contain("You can see ocean water splashing against the steps below you");
        
        // Balcony
        response = await engine.GetResponse("down");
        response.Should().Contain("Underwater");
    }
    
    [Test]
    public async Task DaySeven()
    {
        var engine = GetTarget();
        engine.Context.Day = 7;
        StartHere<Courtyard>();

        // Couryard
        var response = await engine.GetResponse("look");
        response.Should().Contain("From the direction of the stairway comes the sound of ocean surf");
        
        // Winding Stair
        response = await engine.GetResponse("down");
        response.Should().Contain("You can see ocean water splashing against the steps below you");
        
        // Balcony
        response = await engine.GetResponse("down");
        response.Should().Contain("Underwater");
    }
}