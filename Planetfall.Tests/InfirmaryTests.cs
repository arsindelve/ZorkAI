using FluentAssertions;
using GameEngine.Location;
using Model.Interface;
using Moq;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class InfirmaryTests : EngineTestsBase
{
    [Test]
    public async Task ExamineShelves()
    {
        var target = GetTarget();
        StartHere<Infirmary>();
        
        var response = await target.GetResponse("examine shelves");

        response.Should().Contain("The shelves are pretty dusty");
    }
    
    [Test]
    public async Task ExamineEquipment()
    {
        var target = GetTarget();
        StartHere<Infirmary>();
        
        var response = await target.GetResponse("examine equipment");

        response.Should().Contain("so complicated");
    }
    
    [Test]
    public async Task Look_RedSpool()
    {
        var target = GetTarget();
        StartHere<Infirmary>();
        
        var response = await target.GetResponse("look");

        response.Should().Contain("Lying on one of the beds is a small red spool");
    }
    
    [Test]
    public async Task Look_MedicineBottle()
    {
        var target = GetTarget();
        StartHere<Infirmary>();
        
        var response = await target.GetResponse("look");

        response.Should().Contain("On a low shelf is a translucent bottle with a small white label");
    }
    
    [Test]
    public async Task Look_Medicine()
    {
        var target = GetTarget();
        StartHere<Infirmary>();
        
        var response = await target.GetResponse("look");

        response.Should().Contain("At the bottom of the bottle is a small quantity of medicine");
    }
    
    [Test]
    public async Task Look_Medicine_Empty()
    {
        var target = GetTarget();
        StartHere<Infirmary>();
        GetItem<MedicineBottle>().Items.Clear();
        
        var response = await target.GetResponse("look");

        response.Should().NotContain("At the bottom of the bottle is a small quantity of medicine");
    }
    
    [Test]
    public async Task MedicineBottle_Empty_InInventory()
    {
        var target = GetTarget();
        StartHere<Infirmary>();
        GetItem<MedicineBottle>().Items.Clear();
        
        await target.GetResponse("take bottle");
        var response = await target.GetResponse("i");

        response.Should().Contain("A medicine bottle");
        response.Should().NotContain("The medicine bottle contains:");
    }
    
    [Test]
    public async Task MedicineBottle_InInventory()
    {
        var target = GetTarget();
        StartHere<Infirmary>();
        
        await target.GetResponse("take bottle");
        var response = await target.GetResponse("i");

        response.Should().Contain("A medicine bottle");
        response.Should().Contain("The medicine bottle contains:");
        response.Should().Contain("A quantity of medicine");
    }
    
    [Test]
    public async Task Examine_MedicineBottle()
    {
        var target = GetTarget();
        StartHere<Infirmary>();
        
        var response = await target.GetResponse("examine bottle");

        response.Should().Contain("Dizeez supreshun medisin -- eksperimentul");
    }
    
    [Test]
    public async Task Read_MedicineBottle()
    {
        var target = GetTarget();
        StartHere<Infirmary>();
        
        var response = await target.GetResponse("read bottle");

        response.Should().Contain("Dizeez supreshun medisin -- eksperimentul");
    }
    
    [Test]
    public async Task Read_Label()
    {
        var target = GetTarget();
        StartHere<Infirmary>();
        
        var response = await target.GetResponse("read label");

        response.Should().Contain("Dizeez supreshun medisin -- eksperimentul");
    }
    
    [Test]
    public async Task Open_MedicineBottle()
    {
        var target = GetTarget();
        StartHere<Infirmary>();

        var response = await target.GetResponse("open bottle");

        response.Should().Contain("Opened");
    }

    [Test]
    public async Task Floyd_IsPresent_AndTurnedOn_ShouldSpeakAboutLazarus_AfterOneTurn()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var infirmary = StartHere<Infirmary>();
        infirmary.ItemPlacedHere(floyd);
        target.Context.RegisterActor(infirmary);

        // First turn - Floyd should not speak yet
        var response1 = await target.GetResponse("wait");
        response1.Should().NotContain("Lazarus");

        // Second turn - Floyd should speak
        var response2 = await target.GetResponse("wait");
        response2.Should().Contain("Lazarus");
    }

    [Test]
    public async Task Floyd_IsNotTurnedOn_ShouldNotSpeakAboutLazarus()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = false;
        var infirmary = StartHere<Infirmary>();
        infirmary.ItemPlacedHere(floyd);
        target.Context.RegisterActor(infirmary);

        var response1 = await target.GetResponse("wait");
        response1.Should().NotContain("Lazarus");

        var response2 = await target.GetResponse("wait");
        response2.Should().NotContain("Lazarus");
    }

    [Test]
    public async Task Floyd_SpeaksAboutLazarus_OnlyOnce()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var infirmary = StartHere<Infirmary>();
        infirmary.ItemPlacedHere(floyd);
        target.Context.RegisterActor(infirmary);

        // First turn - wait
        await target.GetResponse("wait");

        // Second turn - Floyd speaks
        var response2 = await target.GetResponse("wait");
        response2.Should().Contain("Lazarus");

        // Third turn - Floyd should not speak again (and Floyd has left the room)
        var response3 = await target.GetResponse("wait");
        response3.Should().NotContain("Lazarus");
    }

    [Test]
    public async Task Floyd_NotInRoom_ShouldNotSpeakAboutLazarus()
    {
        var target = GetTarget();
        GetItem<Floyd>().IsOn = true;
        var infirmary = StartHere<Infirmary>();
        target.Context.RegisterActor(infirmary);

        var response1 = await target.GetResponse("wait");
        response1.Should().NotContain("Lazarus");

        var response2 = await target.GetResponse("wait");
        response2.Should().NotContain("Lazarus");
    }

    [Test]
    public async Task Floyd_SpeaksAboutLazarus_AndMedicalRobotBreastPlateAppears()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var infirmary = StartHere<Infirmary>();
        infirmary.ItemPlacedHere(floyd);
        target.Context.RegisterActor(infirmary);

        // First turn - Floyd should not speak yet
        var response1 = await target.GetResponse("wait");
        response1.Should().NotContain("Lazarus");

        // Second turn - Floyd speaks and breastplate appears
        var response2 = await target.GetResponse("wait");
        response2.Should().Contain("Lazarus");

        // Verify breastplate is now in the room
        var breastplate = GetItem<MedicalRobotBreastPlate>();
        breastplate.CurrentLocation.Should().Be(infirmary);
    }

    [Test]
    public async Task MedicalRobotBreastPlate_AppearsAndCanBeSeen()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var infirmary = StartHere<Infirmary>();
        infirmary.ItemPlacedHere(floyd);
        target.Context.RegisterActor(infirmary);

        // Wait two turns for Floyd to speak and item to appear
        await target.GetResponse("wait");
        await target.GetResponse("wait");

        // Verify the item is in the room
        var breastplate = GetItem<MedicalRobotBreastPlate>();
        breastplate.CurrentLocation.Should().Be(infirmary);

        // Verify the item can be seen in the room description
        var look = await target.GetResponse("look");
        look.Should().Contain("breastplate");
    }

    [Test]
    public async Task Floyd_OnlyNeedsToBeInRoomForOneTurnBeforeSpeaking()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var infirmary = StartHere<Infirmary>();
        infirmary.ItemPlacedHere(floyd);
        target.Context.RegisterActor(infirmary);

        // Verify the one-turn delay works correctly
        infirmary.TurnsInInfirmary.Should().Be(0);

        await target.GetResponse("wait");
        infirmary.TurnsInInfirmary.Should().Be(1);

        var response2 = await target.GetResponse("wait");
        response2.Should().Contain("Lazarus");
        infirmary.TurnsInInfirmary.Should().Be(2);
    }

    [Test]
    public async Task Floyd_WandersAfterLazarusScene()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var infirmary = StartHere<Infirmary>();
        infirmary.ItemPlacedHere(floyd);
        target.Context.RegisterActor(infirmary);
        target.Context.RegisterActor(floyd);

        // First turn - Floyd should not speak yet
        await target.GetResponse("wait");
        floyd.IsOffWandering.Should().BeFalse();

        // Second turn - Floyd speaks about Lazarus and starts wandering
        var response2 = await target.GetResponse("wait");
        response2.Should().Contain("Lazarus");

        // Verify Floyd is now wandering
        floyd.IsOffWandering.Should().BeTrue();
        floyd.WanderingTurnsRemaining.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(5);
        floyd.CurrentLocation.Should().BeNull(); // Floyd is not in any location while wandering
        infirmary.Items.Should().NotContain(floyd); // Floyd was removed from the Infirmary
    }

    [Test]
    public async Task Floyd_StartWanderingCalled_AfterLazarusScene()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        var infirmary = StartHere<Infirmary>();
        infirmary.ItemPlacedHere(floyd);
        target.Context.RegisterActor(infirmary);
        target.Context.RegisterActor(floyd);

        // Verify Floyd is in the room and not wandering initially
        floyd.IsOffWandering.Should().BeFalse();
        floyd.CurrentLocation.Should().Be(infirmary);

        // First turn - Floyd should not speak yet
        await target.GetResponse("wait");
        floyd.IsOffWandering.Should().BeFalse();

        // Second turn - Floyd speaks about Lazarus and StartWandering is called
        var response2 = await target.GetResponse("wait");
        response2.Should().Contain("Lazarus");

        // Verify StartWandering was called (Floyd is now wandering)
        floyd.IsOffWandering.Should().BeTrue();
        floyd.WanderingTurnsRemaining.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(5);
        floyd.CurrentLocation.Should().BeNull(); // Floyd is not in any location while wandering
        infirmary.Items.Should().NotContain(floyd); // Floyd was removed from the Infirmary

        // Verify the medical robot breastplate appeared
        var breastplate = GetItem<MedicalRobotBreastPlate>();
        breastplate.CurrentLocation.Should().Be(infirmary);
    }

    [Test]
    public async Task GiveBreastplateToFloyd_FloydWeepsAndWanders()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<MedicalRobotBreastPlate>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().HasEverBeenOn = true;

        var response = await target.GetResponse("give the plate to floyd");

        response.Should().Contain("At first, Floyd is all grins");
    }

    [Test]
    public async Task GiveBreastplateToFloyd_FloydNotInRoom_CannotGive()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<MedicalRobotBreastPlate>();
        GetItem<Floyd>().IsOn = true;

        // Move to a different room (Floyd doesn't follow since not turned on properly)
        await target.GetResponse("w");

        // Try to give it to Floyd (he's not in this room)
        var response = await target.GetResponse("give breastplate to floyd");

        // Should get generic response since Floyd isn't here
        response.Should().NotContain("all grins");
        response.Should().NotContain("weeping");
        GetItem<Floyd>().IsOffWandering.Should().BeFalse();
    }

    [Test]
    public async Task GiveBreastplateToFloyd_FloydIsOff_NoResponse()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<MedicalRobotBreastPlate>();
        GetItem<Floyd>().IsOn = false;

        var response = await target.GetResponse("give breastplate to floyd");

        response.Should().NotContain("all grins");
        response.Should().NotContain("weeping");
        GetItem<Floyd>().IsOffWandering.Should().BeFalse();
    }

    [Test]
    public async Task GiveBreastplateToFloyd_FloydIsDead_NoResponse()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<MedicalRobotBreastPlate>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().HasDied = true;

        var response = await target.GetResponse("give breastplate to floyd");

        response.Should().NotContain("all grins");
        response.Should().NotContain("weeping");
        GetItem<Floyd>().IsOffWandering.Should().BeFalse();
    }

    [Test]
    public async Task GiveBreastplateToFloyd_BreastplateDroppedInCurrentRoom()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<MedicalRobotBreastPlate>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().HasEverBeenOn = true;

        await target.GetResponse("give the breastplate to floyd");

        // Verify breastplate is on the floor, not in inventory
        target.Context.Items.Should().NotContain(GetItem<MedicalRobotBreastPlate>());
        ((LocationBase)target.Context.CurrentLocation).Items.Should().Contain(GetItem<MedicalRobotBreastPlate>());

        // Verify we can see it in the room
        var look = await target.GetResponse("look");
        look.Should().Contain("breastplate");
    }
}