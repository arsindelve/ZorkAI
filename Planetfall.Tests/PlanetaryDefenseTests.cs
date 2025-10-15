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
    
    [TestCase("first")]
    [TestCase("third")]
    [TestCase("fourth")]
    public async Task Remove_InsidePanel_Shock(string name)
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;

        var response = await target.GetResponse($"remove {name}");

        response.Should().Contain("You jerk your hand back as you receive a powerful shock from the fromitz board.");
    }
    
    [Test]
    public async Task Remove_NotInsidePanel_Shock()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<SecondFromitzBoard>().CurrentLocation = GetLocation<PlanetaryDefense>();

        var response = await target.GetResponse($"remove second");

        response.Should().NotContain("You jerk your hand back as you receive a powerful shock from the fromitz board.");
    }
    
    [Test]
    public async Task Take_NotInsidePanel_NoShock()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetLocation<PlanetaryDefense>().ItemPlacedHere(GetItem<SecondFromitzBoard>());

        var response = await target.GetResponse("take second");

        response.Should().Contain("Taken");
    }
    
    [Test]
    public async Task Take_InsidePanel_NoShock()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;

        var response = await target.GetResponse("take second");

        response.Should().Contain("The fromitz board slides out of the panel, producing an empty socket for another board");
    }
    
    [Test]
    public async Task InInventory()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetLocation<PlanetaryDefense>().ItemPlacedHere(GetItem<SecondFromitzBoard>());

        await target.GetResponse("take second");
        var response = await target.GetResponse("inventory");

        response.Should().Contain("  A fried seventeen-centimeter fromitz board");
    }
    
    [Test]
    public async Task Look_NotInsidePanel()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetLocation<PlanetaryDefense>().ItemPlacedHere(GetItem<SecondFromitzBoard>());

        var response = await target.GetResponse("look");

        response.Should().Contain("There is a fried seventeen-centimeter fromitz board here.");
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

        response.Should()
            .Contain(
                "Do you mean the first fromitz board, the second fromitz board, the third fromitz board or the fourth fromitz board?");
    }
}