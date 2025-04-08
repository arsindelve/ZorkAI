using FluentAssertions;
using Planetfall.Item.Lawanda;
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
}