using FluentAssertions;
using GameEngine;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda.PlanetaryDefense;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class PlanetaryDefenseTests : EngineTestsBase
{
    [Test]
    public async Task Look()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        
        string? response = await target.GetResponse("look");
        response.Should().Contain("One light, blinking quickly");
        response.Should().Contain("Surkit Boord Faalyur. WORNEENG: xis boord kuntroolz xe diskriminaashun surkits.");
    }
    
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
        GetItem<FriedFromitzBoard>().CurrentLocation = GetLocation<PlanetaryDefense>();

        var response = await target.GetResponse($"remove second");

        response.Should().NotContain("You jerk your hand back as you receive a powerful shock from the fromitz board.");
    }
    
    [Test]
    public async Task Take_NotInsidePanel_NoShock()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetLocation<PlanetaryDefense>().ItemPlacedHere(GetItem<FriedFromitzBoard>());

        var response = await target.GetResponse("take fried");

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
        GetLocation<PlanetaryDefense>().ItemPlacedHere(GetItem<FriedFromitzBoard>());

        await target.GetResponse("take fried");
        var response = await target.GetResponse("inventory");

        response.Should().Contain("  A fried seventeen-centimeter fromitz board");
    }
    
    [Test]
    public async Task InInventory_Examine()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        
        await target.GetResponse("take second");
        var response = await target.GetResponse("examine fried");

        response.Should().Contain("Like most fromitz boards, it is a twisted maze of silicon circuits. It is square, approximately seventeen centimeters on each side.");
        response.Should().Contain("This one is a bit blackened around the edges, though");
    }
    
    [Test]
    public async Task InPanel_Examine()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        
        var response = await target.GetResponse("examine second");

        response.Should().Contain("Like most fromitz boards, it is a twisted maze of silicon circuits. It is square, approximately seventeen centimeters on each side.");
        response.Should().NotContain("This one is a bit blackened around the edges, though");
    }
    
    [Test]
    public async Task Look_NotInsidePanel()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetLocation<PlanetaryDefense>().ItemPlacedHere(GetItem<FriedFromitzBoard>());

        var response = await target.GetResponse("look");

        response.Should().Contain("There is a fried seventeen-centimeter fromitz board here.");
    }
    
    [Test]
    public async Task PutInPanel_Full()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        Take<Canteen>();

        var response = await target.GetResponse("put canteen in panel");

        response.Should().Contain("There's no room");
    }
    
    [Test]
    public async Task PutInPanel_WrongItem()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        Take<Canteen>();
        Take<FriedFromitzBoard>();

        var response = await target.GetResponse("put canteen in panel");

        response.Should().Contain("The canteen doesn't fit.");
    }
    
    [Test]
    public async Task PutInPanel_Fried()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        Take<FriedFromitzBoard>();

        var response = await target.GetResponse("put fried in panel");

        response.Should().Contain("The card clicks neatly into the socket");
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
    
    [Test]
    public async Task CompleteSolution_CannotPullBoardBackOut()
    {
        var target = GetTarget();
        await Solve(target);

        string? response = await target.GetResponse("take second");
        response.Should().Contain("You jerk your hand back as you receive a powerful shock from the fromitz board");
    }
    
    [Test]
    public async Task CompleteSolution_TakeShiny()
    {
        var target = GetTarget();
        await Solve(target);

        string? response = await target.GetResponse("take shiny");
        response!.Trim().Should().BeEmpty();
    }
    
    [Test]
    public async Task CompleteSolution_Look()
    {
        var target = GetTarget();
        await Solve(target);

        string? response = await target.GetResponse("look");
        response.Should().NotContain("One light, blinking quickly");
    }
    
    [Test]
    public async Task CompleteSolution_PutFriedBack()
    {
        var target = GetTarget();
        await Solve(target);

        string? response = await target.GetResponse("put fried in panel");
        response.Should().Contain("no room");
    }

    [Test]
    public async Task CrackedBoard_CanBeTaken()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        var response = await target.GetResponse("take cracked");

        response.Should().Contain("Taken");
        target.Context.Items.Should().Contain(GetItem<CrackedFromitzBoard>());
    }

    [Test]
    public async Task CrackedBoard_CanBePutInPanel()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        Take<CrackedFromitzBoard>();
        Take<FriedFromitzBoard>();

        var response = await target.GetResponse("put cracked in panel");

        response.Should().Contain("The card clicks neatly into the socket");
        GetItem<FromitzAccessPanel>().HasItem<CrackedFromitzBoard>().Should().BeTrue();
    }

    [Test]
    public async Task CrackedBoard_CanBeRemovedFromPanel()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        Take<CrackedFromitzBoard>();
        Take<FriedFromitzBoard>();

        // Put cracked board in panel (now it's "second")
        await target.GetResponse("put cracked in panel");

        // Remove it
        var response = await target.GetResponse("remove second");

        response.Should().NotContain("shock");
        response.Should().Contain("slides out of the panel");
        target.Context.Items.Should().Contain(GetItem<CrackedFromitzBoard>());
    }

    [Test]
    public async Task CrackedBoard_DoesNotSolvePuzzle()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        Take<CrackedFromitzBoard>();
        Take<FriedFromitzBoard>();

        var response = await target.GetResponse("put cracked in panel");

        response.Should().Contain("The card clicks neatly into the socket");
        response.Should().NotContain("The warning lights stop flashing");
        target.Context.Score.Should().Be(0);
    }

    [Test]
    public async Task CrackedBoard_NounChanges_InPanel()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        Take<CrackedFromitzBoard>();
        Take<FriedFromitzBoard>();

        await target.GetResponse("put cracked in panel");

        var response = await target.GetResponse("examine panel");

        response.Should().Contain("second seventeen-centimeter fromitz board");
    }

    [Test]
    public async Task CrackedBoard_NounChanges_OutOfPanel()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        GetLocation<PlanetaryDefense>().ItemPlacedHere(GetItem<CrackedFromitzBoard>());

        var response = await target.GetResponse("look");

        response.Should().Contain("cracked seventeen-centimeter fromitz board");
    }

    [Test]
    public async Task CrackedBoard_ExaminationDescription()
    {
        var target = GetTarget();
        StartHere<PlanetaryDefense>();
        Take<CrackedFromitzBoard>();

        var response = await target.GetResponse("examine cracked");

        response.Should().Contain("This one looks as though it's been dropped");
    }

    private async Task Solve(GameEngine<PlanetfallGame, PlanetfallContext> target)
    {
        StartHere<PlanetaryDefense>();
        GetItem<FromitzAccessPanel>().IsOpen = true;
        Take<ShinyFromitzBoard>();

        await target.GetResponse("remove second");
        var response = await target.GetResponse("put shiny in panel");
        response.Should().Contain("The card clicks neatly into the socket");
        response.Should().Contain("The warning lights stop flashing");
        target.Context.Score.Should().Be(6);
    }
}