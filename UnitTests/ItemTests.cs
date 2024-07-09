using Model.Interface;
using Model.Item;

namespace UnitTests;

public class ItemTests : EngineTestsBase
{
    [Test]
    public void GetEverythingInALocationDoesNotGetThingsInsideClosedContainer()
    {
        var engine = GetTarget();
        var here = engine.Context.CurrentLocation as ICanHoldItems;
        here!.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Mailbox>());
        here.GetAllItemsRecursively.Count.Should().Be(1);
        here.GetAllItemsRecursively.Should().NotContain(Repository.GetItem<Leaflet>());
    }

    [Test]
    public async Task GetEverythingInALocationGetItemsInAnOpenContainer()
    {
        var engine = GetTarget();
        var here = engine.Context.CurrentLocation as ICanHoldItems;
        // Act
        await engine.GetResponse("open mailbox");
        here!.GetAllItemsRecursively.Count.Should().Be(2);
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Leaflet>());
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Mailbox>());
    }

    [Test]
    public async Task GetEverythingInALocationGetItemsInAnOpenContainerAndOnTheGround()
    {
        var engine = GetTarget();
        Repository.GetLocation<WestOfHouse>().ItemPlacedHere(Repository.GetItem<Sword>());
        var here = engine.Context.CurrentLocation as ICanHoldItems;
        // Act
        await engine.GetResponse("open mailbox");
        here!.GetAllItemsRecursively.Count.Should().Be(3);
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Leaflet>());
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Mailbox>());
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Sword>());
    }

    [Test]
    public async Task ClosedContainerInsideAnotherContainer()
    {
        var engine = GetTarget();
        Repository.GetLocation<WestOfHouse>().ItemPlacedHere(Repository.GetItem<Sword>());
        Repository.GetItem<Mailbox>().ItemPlacedHere(Repository.GetItem<BrownSack>());
        var here = engine.Context.CurrentLocation as ICanHoldItems;
        // Act
        await engine.GetResponse("open mailbox");
        here!.GetAllItemsRecursively.Count.Should().Be(4);
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Leaflet>());
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Mailbox>());
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Sword>());
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<BrownSack>());
    }

    [Test]
    public async Task OpenContainerInsideAnotherContainer()
    {
        var engine = GetTarget();
        Repository.GetLocation<WestOfHouse>().ItemPlacedHere(Repository.GetItem<Sword>());
        Repository.GetItem<Mailbox>().ItemPlacedHere(Repository.GetItem<BrownSack>());
        var here = engine.Context.CurrentLocation as ICanHoldItems;
        // Act
        await engine.GetResponse("open mailbox");
        await engine.GetResponse("open sack");
        here!.GetAllItemsRecursively.Count.Should().Be(6);
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Leaflet>());
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Mailbox>());
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Sword>());
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<BrownSack>());
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Garlic>());
        here.GetAllItemsRecursively.Should().Contain(Repository.GetItem<Lunch>());
    }

    [Test]
    public async Task PickingSomethingUpPutsItInInventory()
    {
        var engine = GetTarget();
        engine.Context.Items.Should().BeEmpty();

        // Act
        await engine.GetResponse("open mailbox");
        await engine.GetResponse("take leaflet");

        // Assert
        engine.Context.Items.Should().Contain(Repository.GetItem<Leaflet>());
        Repository.GetItem<Leaflet>().CurrentLocation.Should().Be(engine.Context);
    }

    [Test]
    public async Task ItemsInInventoryDescription()
    {
        var engine = GetTarget();
        engine.Context.Items.Should().BeEmpty();

        // Act
        await engine.GetResponse("open mailbox");
        await engine.GetResponse("take leaflet");
        var response = await engine.GetResponse("i");

        // Assert
        response.Should().Contain("leaflet");
    }

    [Test]
    public async Task PickingSomethingUpRemovesItFromWhereItWas()
    {
        var engine = GetTarget();
        var location = Repository.GetItem<Mailbox>();
        location.HasItem<Leaflet>().Should().BeTrue();

        // Act
        await engine.GetResponse("open mailbox");
        await engine.GetResponse("take leaflet");

        // Assert
        location.HasItem<Leaflet>().Should().BeFalse();
        Repository.GetItem<Leaflet>().CurrentLocation.Should().Be(engine.Context);
    }

    [Test]
    public async Task DroppingSomethingTakesItOutOfInventory()
    {
        var engine = GetTarget();

        // Act
        await engine.GetResponse("open mailbox");
        await engine.GetResponse("take leaflet");
        await engine.GetResponse("drop leaflet");

        // Assert
        var location = Repository.GetLocation<WestOfHouse>();
        var item = Repository.GetItem<Leaflet>();
        engine.Context.Items.Should().BeEmpty();
        item.CurrentLocation.Should().Be(location);
        location.HasItem<Leaflet>().Should().BeTrue();
    }

    [Test]
    public async Task NounMatch_InInventory()
    {
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        // Act
        await engine.GetResponse("take sword");

        // Assert
        engine.Context.HasItem<Sword>().Should().BeTrue();
        engine.Context.HasMatchingNoun("sword").Should().BeTrue();
    }

    [Test]
    public async Task NounMatch_OnTheFloor()
    {
        var engine = GetTarget();
        var location = Repository.GetLocation<LivingRoom>();
        engine.Context.CurrentLocation = location;

        // Act
        await engine.GetResponse("examine sword");

        // Assert
        location.HasItem<Sword>().Should().BeTrue();
        location.HasMatchingNoun("sword").Should().BeTrue();
    }

    [Test]
    public async Task NounMatch_OnTheFloor_AfterTakingIt()
    {
        var engine = GetTarget();
        var location = Repository.GetLocation<LivingRoom>();
        engine.Context.CurrentLocation = location;

        // Act
        await engine.GetResponse("take sword");

        // Assert
        location.HasItem<Sword>().Should().BeFalse();
        location.HasMatchingNoun("sword").Should().BeFalse();
    }

    [Test]
    public async Task NounMatch_OnTheFloor_AfterDroppingIt()
    {
        var engine = GetTarget();
        var livingRoom = Repository.GetLocation<LivingRoom>();
        engine.Context.CurrentLocation = livingRoom;

        // Act
        await engine.GetResponse("take sword");
        await engine.GetResponse("E");
        await engine.GetResponse("drop sword");

        // Assert
        engine.Context.CurrentLocation.HasItem<Sword>().Should().BeTrue();
        engine.Context.CurrentLocation.HasMatchingNoun("sword").Should().BeTrue();
        livingRoom.HasItem<Sword>().Should().BeFalse();
        livingRoom.HasMatchingNoun("sword").Should().BeFalse();
    }

    [Test]
    public async Task NounMatch_Nested()
    {
        var engine = GetTarget();

        engine.Context.CurrentLocation.HasMatchingNoun("mailbox").Should().BeTrue();
        engine.Context.CurrentLocation.HasMatchingNoun("leaflet").Should().BeFalse();

        await engine.GetResponse("open mailbox");

        engine.Context.CurrentLocation.HasMatchingNoun("leaflet").Should().BeTrue();
    }

    [Test]
    public async Task NounMatch_Container_Nested()
    {
        var engine = GetTarget();

        // Assert
        Repository.GetItem<Mailbox>().HasMatchingNoun("leaflet", false).Should().BeFalse();

        // Act
        await engine.GetResponse("open mailbox");

        // Assert 
        Repository.GetItem<Mailbox>().HasMatchingNoun("leaflet").Should().BeTrue();
    }

    [Test]
    public void Repository_DoesNotExistInTheStory()
    {
        Repository.ItemExistsInTheStory("unicorn").Should().BeFalse();
    }

    [Test]
    public void Repository_DoesNotExistInTheStory_Blank()
    {
        Repository.ItemExistsInTheStory("").Should().BeFalse();
    }

    [Test]
    public async Task ClosedContainer_ShowsContents_Transparent()
    {
        var engine = GetTarget();
        var location = Repository.GetLocation<Kitchen>();
        engine.Context.CurrentLocation = location;

        // Act
        var response = await engine.GetResponse("look");

        response.Should().Contain("water");
    }

    [Test]
    public async Task ClosedContainer_DoesNotShowContents_NotTransparent()
    {
        var engine = GetTarget();
        var location = Repository.GetLocation<WestOfHouse>();
        engine.Context.CurrentLocation = location;

        // Act
        var response = await engine.GetResponse("look");

        response.Should().Contain("mailbox");
        response.Should().NotContain("leaflet");
    }

    [Test]
    public void TorchIsALightSource()
    {
        var engine = GetTarget();
        engine.Context.Take(Repository.GetItem<Torch>());

        // Act, Assert
        engine.Context.HasLightSource.Should().BeTrue();
    }

    [Test]
    public void ListOfItemsInContainer_ThreeItems()
    {
        Repository.Reset();
        var machine = Repository.GetItem<Machine>();
        machine.ItemPlacedHere(Repository.GetItem<Diamond>());
        machine.ItemPlacedHere(Repository.GetItem<PileOfLeaves>());
        machine.ItemPlacedHere(Repository.GetItem<Emerald>());

        string list = machine.SingleLineListOfItems();

        list.Should().Be("a huge diamond, a pile of leaves and a large emerald");
    }
    
    [Test]
    public void ListOfItemsInContainer_TwoItems()
    {
        Repository.Reset();
        var machine = Repository.GetItem<Machine>();
        machine.ItemPlacedHere(Repository.GetItem<Diamond>());
        machine.ItemPlacedHere(Repository.GetItem<PileOfLeaves>());

        string list = machine.SingleLineListOfItems();

        list.Should().Be("a huge diamond and a pile of leaves");
    }
    
    [Test]
    public void ListOfItemsInContainer_OneItem()
    {
        Repository.Reset();
        var machine = Repository.GetItem<Machine>();
        machine.ItemPlacedHere(Repository.GetItem<Diamond>());

        string list = machine.SingleLineListOfItems();

        list.Should().Be("a huge diamond");
    }
    
    [Test]
    public void ListOfItemsInContainer_NoItems()
    {
        Repository.Reset();
        var machine = Repository.GetItem<Machine>();

        string list = machine.SingleLineListOfItems();

        list.Should().Be("");
    }
    
    [Test]
    public void GetAllItemsRecursively_ThreeItems()
    {
        Repository.Reset();
        Repository.Reset();
        var machine = Repository.GetItem<Machine>();
        machine.IsOpen = true;
        machine.ItemPlacedHere(Repository.GetItem<Diamond>());
        machine.ItemPlacedHere(Repository.GetItem<PileOfLeaves>());
        machine.ItemPlacedHere(Repository.GetItem<Emerald>());
        
        List<IItem> list = machine.GetAllItemsRecursively;

        list.Should().Contain(Repository.GetItem<Diamond>());
        list.Should().Contain(Repository.GetItem<PileOfLeaves>());
        list.Should().Contain(Repository.GetItem<Emerald>());
    }
    
    [Test]
    public void GetAllItemsRecursively_ThreeItems_Closed()
    {
        Repository.Reset();
        Repository.Reset();
        var machine = Repository.GetItem<Machine>();
        machine.IsOpen = false;
        machine.ItemPlacedHere(Repository.GetItem<Diamond>());
        machine.ItemPlacedHere(Repository.GetItem<PileOfLeaves>());
        machine.ItemPlacedHere(Repository.GetItem<Emerald>());
        
        List<IItem> list = machine.GetAllItemsRecursively;

        list.Should().BeEmpty();
    }
    
    [Test]
    public void GetAllItemsRecursively_ContainerInsideContainer()
    {
        Repository.Reset();
        Repository.Reset();
        var machine = Repository.GetItem<Machine>();
        var sack = Repository.GetItem<BrownSack>();
        machine.ItemPlacedHere(Repository.GetItem<Emerald>());
        machine.IsOpen = true;
        sack.IsOpen = true;
        machine.ItemPlacedHere(sack);
        
        List<IItem> list = machine.GetAllItemsRecursively;

        list.Should().Contain(Repository.GetItem<Emerald>());
        list.Should().Contain(Repository.GetItem<Lunch>());
        list.Should().Contain(Repository.GetItem<Garlic>());
        list.Should().Contain(Repository.GetItem<BrownSack>());
    }
    
    [Test]
    public void GetAllItemsRecursively_ContainerInsideContainer_InnerContainerIsClosed()
    {
        Repository.Reset();
        Repository.Reset();
        var machine = Repository.GetItem<Machine>();
        var sack = Repository.GetItem<BrownSack>();
        machine.ItemPlacedHere(Repository.GetItem<Emerald>());
        machine.IsOpen = true;
        sack.IsOpen = false;
        machine.ItemPlacedHere(sack);
        
        List<IItem> list = machine.GetAllItemsRecursively;

        list.Should().Contain(Repository.GetItem<Emerald>());
        list.Should().Contain(Repository.GetItem<BrownSack>());
    }
    
    [Test]
    public void Context_GetAllItemsRecursively_ContainerInsideContainer_InnerContainerIsClosed()
    {
        Repository.Reset();
        Repository.Reset();
        var context = GetTarget().Context;
        var sack = Repository.GetItem<BrownSack>();
        context.ItemPlacedHere(Repository.GetItem<Emerald>());
        sack.IsOpen = false;
        context.ItemPlacedHere(sack);
        
        List<IItem> list = context.GetAllItemsRecursively;

        list.Should().Contain(Repository.GetItem<Emerald>());
        list.Should().Contain(Repository.GetItem<BrownSack>());
    }
    
    [Test]
    public void Context_GetAllItemsRecursively_ContainerInsideContainer()
    {
        Repository.Reset();
        Repository.Reset();
        var context = GetTarget().Context;
        var sack = Repository.GetItem<BrownSack>();
        context.ItemPlacedHere(Repository.GetItem<Emerald>());
        sack.IsOpen = true;
        context.ItemPlacedHere(sack);
        
        List<IItem> list = context.GetAllItemsRecursively;

        list.Should().Contain(Repository.GetItem<Emerald>());
        list.Should().Contain(Repository.GetItem<BrownSack>());
        list.Should().Contain(Repository.GetItem<Lunch>());
        list.Should().Contain(Repository.GetItem<Garlic>());
    }
}