using Model.Interface;
using ZorkOne;

namespace UnitTests.ZorkITests;

public class MachineTests : EngineTestsBase
{
    private Machine _machine;

    // List things in the machine
    [Test]
    public async Task ListThingsInTheMachine_Nothing()
    {
        var target = Setup();

        var response = await target.GetResponse("open lid");

        response.Should().Contain("The lid opens");
    }

    [Test]
    public async Task ListThingsInTheMachine_OneItem()
    {
        var target = Setup();
        _machine.ItemPlacedHere(Repository.GetItem<Garlic>());

        var response = await target.GetResponse("open lid");

        response.Should().Contain("The lid opens revealing a clove of garlic");
    }
    
    [Test]
    public async Task ListThingsInTheMachine_TwoItems()
    {
        var target = Setup();
        _machine.ItemPlacedHere(Repository.GetItem<Garlic>());
        _machine.ItemPlacedHere(Repository.GetItem<Sword>());

        var response = await target.GetResponse("open lid");

        response.Should().Contain("The lid opens revealing a clove of garlic and a glamdring");
    }
    
    [Test]
    public async Task ListThingsInTheMachine_ThreeItems()
    {
        var target = Setup();
        _machine.ItemPlacedHere(Repository.GetItem<Garlic>());
        _machine.ItemPlacedHere(Repository.GetItem<Sword>());
        _machine.ItemPlacedHere(Repository.GetItem<Knife>());

        var response = await target.GetResponse("open lid");

        response.Should().Contain("The lid opens revealing a clove of garlic, a glamdring and a nasty knife");
    }
    
    // Try to turn it on with fingers
    
    
    [Test]
    public async Task TurnSwitch_BareHands()
    {
        var target = Setup();

        var response = await target.GetResponse("turn switch");

        response.Should().Contain("Your bare hands don't appear to be enough");
    }
    
    [Test]
    public async Task TurnSwitch_NotHoldingScrewdriver()
    {
        var target = Setup();
        ((ICanHoldItems)target.Context.CurrentLocation).ItemPlacedHere(Repository.GetItem<Screwdriver>());

        var response = await target.GetResponse("turn switch with screwdriver");

        response.Should().Contain("You don't have the screwdriver");
    }
    
    [Test]
    public async Task TurnSwitch_LidOpen()
    {
        var target = Setup();
        target.Context.ItemPlacedHere(Repository.GetItem<Screwdriver>());
        _machine.IsOpen = true;

        var response = await target.GetResponse("turn switch with screwdriver");

        response.Should().Contain("The machine doesn't seem to want to do anything");
    }
    
    [Test]
    public async Task TurnSwitch_Empty()
    {
        var target = Setup();
        target.Context.ItemPlacedHere(Repository.GetItem<Screwdriver>());
        _machine.IsOpen = true;

        var response = await target.GetResponse("turn switch with screwdriver");

        response.Should().Contain("The machine doesn't seem to want to do anything");
    }
    
    private GameEngine<ZorkI, ZorkIContext> Setup()
    {
      
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MachineRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        _machine = Repository.GetItem<Machine>();
        return target;
    }

    

  
    // Try to turn on with lid open
    // Item turns to slag
    // Nothing in the machine
    // Coal turns to diamond
    // Coal + other item does not turn to slag
    
    
}