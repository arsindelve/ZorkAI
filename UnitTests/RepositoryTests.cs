using GameEngine;
using Model.Interface;
using Model.Item;
using Model.Location;
using Moq.Language;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Lawanda;

namespace UnitTests;

public class RepositoryTests
{
    [SetUp]
    public void Setup()
    {
        // Clear Repository state before each test if necessary
        // This depends on how the Repository manages state between tests
    }

    [Test]
    public void GetLocation_WithValidName_ReturnsCorrectLocation()
    {
        // Arrange - use a location known to exist in the game
        var locationName = "Rec Area";

        // Act
        var result = Repository.GetLocation(locationName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RecArea>();
        result!.Name.Should().Be("Rec Area");
    }

    [Test]
    public void GetLocation_WithValidNameDifferentCase_ReturnsCorrectLocation()
    {
        // Arrange
        var locationName = "rEc aReA"; // Mixed case

        // Act
        var result = Repository.GetLocation(locationName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RecArea>();
    }

    [Test]
    public void GetLocation_WithNullName_ReturnsNull()
    {
        // Arrange
        string? locationName = null;

        // Act
        var result = Repository.GetLocation(locationName);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetLocation_WithEmptyName_ReturnsNull()
    {
        // Arrange
        var locationName = string.Empty;

        // Act
        var result = Repository.GetLocation(locationName);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetLocation_WithWhitespaceName_ReturnsNull()
    {
        // Arrange
        var locationName = "   ";

        // Act
        var result = Repository.GetLocation(locationName);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetLocation_WithNonExistentName_ReturnsNull()
    {
        // Arrange
        var locationName = "NonExistentLocation";

        // Act
        var result = Repository.GetLocation(locationName);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetLocation_WithPartialName_ReturnsNull()
    {
        // Arrange
        var locationName = "Rec"; // Only part of "Rec Area"

        // Act
        var result = Repository.GetLocation(locationName);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetLocation_GenericMethod_ReturnsCorrectType()
    {
        // Act
        var result = Repository.GetLocation<RecArea>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RecArea>();
    }
    
    [Test]
    public void GetItem_GenericMethod_ReturnsCorrectType()
    {
        // Act
        var result = Repository.GetItem<PieceOfPaper>();
    
        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PieceOfPaper>();
    }
    
    [Test]
    public void GetItem_GenericMethod_ReturnsSameInstanceOnSecondCall()
    {
        // Act - Get the item twice
        var firstResult = Repository.GetItem<PieceOfPaper>();
        var secondResult = Repository.GetItem<PieceOfPaper>();
    
        // Assert - Should be the same instance (singleton pattern)
        secondResult.Should().BeSameAs(firstResult);
    }
    
    [Test]
    public void GetLocation_GenericMethod_ReturnsSameInstanceOnSecondCall()
    {
        // Act - Get the location twice
        var firstResult = Repository.GetLocation<Infirmary>();
        var secondResult = Repository.GetLocation<Infirmary>();
    
        // Assert - Should be the same instance (singleton pattern)
        secondResult.Should().BeSameAs(firstResult);
    }
    
    [Test]
    public void GetItem_StringMethod_WithValidNoun_ReturnsCorrectItem()
    {
        // Arrange - Make sure the item exists in repository first
        Repository.GetItem<PieceOfPaper>();
    
        // Act
        var result = Repository.GetItem("paper");
    
        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PieceOfPaper>();
    }
    
    [Test]
    public void GetItem_StringMethod_WithInvalidNoun_ReturnsNull()
    {
        // Act
        var result = Repository.GetItem("non-existent-item");
    
        // Assert
        result.Should().BeNull();
    }
    
    [Test]
    public void GetItem_StringMethod_WithNullOrEmpty_ReturnsNull()
    {
        // Act
        var resultNull = Repository.GetItem((string?)null);
        var resultEmpty = Repository.GetItem("");
        var resultWhitespace = Repository.GetItem("   ");
    
        // Assert
        resultNull.Should().BeNull();
        resultEmpty.Should().BeNull();
        resultWhitespace.Should().BeNull();
    }
    
    [Test]
    public void GetItem_StringMethod_TrimmingBehavior_RemovesLeadingAndTrailingSpaces()
    {
        // Arrange
        Repository.GetItem<PieceOfPaper>();
        var itemName = "  paper  "; // Extra spaces
    
        // Act
        var result = Repository.GetItem(itemName);
    
        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PieceOfPaper>();
    }
    
    [Test]
    public void ItemExistsInTheStory_WithValidItem_ReturnsTrue()
    {
        // Arrange
        Repository.GetItem<PieceOfPaper>();
    
        // Act
        var result = Repository.ItemExistsInTheStory("paper");
    
        // Assert
        result.Should().BeTrue();
    }
    
    [Test]
    public void ItemExistsInTheStory_WithInvalidItem_ReturnsFalse()
    {
        // Act
        var result = Repository.ItemExistsInTheStory("non-existent-item");
    
        // Assert
        result.Should().BeFalse();
    }
    
    [Test]
    public void ItemExistsInTheStory_WithNullOrEmpty_ReturnsFalse()
    {
        // Act
        var resultNull = Repository.ItemExistsInTheStory(null);
        var resultEmpty = Repository.ItemExistsInTheStory("");
    
        // Assert
        resultNull.Should().BeFalse();
        resultEmpty.Should().BeFalse();
    }
    
    [Test]
    public void Reset_ClearsAllItemsAndLocations()
    {
        // Arrange - Create some items and locations
        var item = Repository.GetItem<PieceOfPaper>();
        var location = Repository.GetLocation<Infirmary>();
    
        // Act
        Repository.Reset();
        
        // Get the same items again - should be new instances
        var newItem = Repository.GetItem<PieceOfPaper>();
        var newLocation = Repository.GetLocation<Infirmary>();
    
        // Assert - Should be different instances after reset
        newItem.Should().NotBeSameAs(item);
        newLocation.Should().NotBeSameAs(location);
    }
    
    [Test]
    public void GetNouns_ReturnsArrayOfNounsFromItems()
    {
        // Act
        var nouns = Repository.GetNouns("Planetfall");
    
        // Assert
        nouns.Should().NotBeEmpty();
        nouns.Should().Contain("paper"); // PieceOfPaper's noun
    }
    
    [Test]
    public void GetContainers_ReturnsArrayOfNounsFromContainers()
    {
        // Act
        var containers = Repository.GetContainers("Planetfall");

        // Assert
        containers.Should().NotBeNull();
    }

    // Issue #241: the god-mode bulk loaders (LoadAllItems / LoadAllLocations) used to replace the
    // whole repository dictionary with fresh, un-Init()-ed instances. That broke god mode two ways:
    // containers came back empty (Init never ran) and any restored/live session state was discarded.
    [Test]
    public void LoadAllItems_InitializesContainers_SoCardboardBoxStartsWithItsContents()
    {
        Repository.Reset();

        Repository.LoadAllItems("Planetfall");

        var box = Repository.GetItem<CardboardBox>();
        box.Items.Should().NotBeEmpty();
        box.Items.Should().Contain(i => i is GoodBedistor);
    }

    [Test]
    public void LoadAllItems_PreservesExistingInstancesAndTheirState()
    {
        Repository.Reset();

        // Simulate a restored, in-progress session: an item is already loaded with mutated state.
        var box = Repository.GetItem<CardboardBox>();
        var bedistor = Repository.GetItem<GoodBedistor>();
        box.Items.Clear(); // the player emptied the box during play

        Repository.LoadAllItems("Planetfall");

        Repository.GetItem<CardboardBox>().Should().BeSameAs(box);
        Repository.GetItem<GoodBedistor>().Should().BeSameAs(bedistor);
        box.Items.Should().BeEmpty(); // live state preserved, not rebuilt from defaults
    }

    [Test]
    public void LoadAllLocations_PreservesExistingInstances()
    {
        Repository.Reset();

        var rec = Repository.GetLocation<RecArea>();

        Repository.LoadAllLocations("Planetfall");

        Repository.GetLocation<RecArea>().Should().BeSameAs(rec);
    }
}
