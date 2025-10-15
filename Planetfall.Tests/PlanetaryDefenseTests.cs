using FluentAssertions;
using Planetfall.Item.Lawanda.PlanetaryDefense;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class PlanetaryDefenseTests : EngineTestsBase
{
    [Test]
    public async Task ExaminePanel_Closed()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        
        var response = await target.GetResponse("examine panel");

        response.Should().Contain("The access panel is closed");
    }
    
    [Test]
    public async Task ExaminePanel_Open()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        
        var response = await target.GetResponse("examine panel");

        response.Should().Contain("The access panel contains:");
        response.Should().Contain("  A first seventeen-centimeter fromitz board");
        response.Should().Contain("  A second seventeen-centimeter fromitz board");
        response.Should().Contain("  A third seventeen-centimeter fromitz board");
        response.Should().Contain("  A fourth seventeen-centimeter fromitz board");
    }
    
    [Test]
    [TestCase("first")]
    [TestCase("third")]
    [TestCase("fourth")]
    public async Task Take_Shock(string name)
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        
        var response = await target.GetResponse($"take {name}");

        response.Should().Contain("You jerk your hand back as you receive a powerful shock from the fromitz board.");
    }
    
    [Test]
    [TestCase("board")]
    [TestCase("fromitz board")]
    [TestCase("fromitz")]
    public async Task Take_Ambiguous(string noun)
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        
        var response = await target.GetResponse($"take {noun}");

        response.Should().Contain("Do you mean the first fromitz board, the second fromitz board, the third fromitz board or the fourth fromitz board?");
    }
}