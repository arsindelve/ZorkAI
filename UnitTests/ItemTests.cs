using ZorkOne.Item;
using ZorkOne.Location;

namespace UnitTests;

public class ItemTests : EngineTestsBase
{
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
        WestOfHouse location = Repository.GetLocation<WestOfHouse>();
        Leaflet item = Repository.GetItem<Leaflet>();
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
        Repository.GetItem<Mailbox>().HasMatchingNoun("leaflet").Should().BeFalse();
        
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
    
  
}