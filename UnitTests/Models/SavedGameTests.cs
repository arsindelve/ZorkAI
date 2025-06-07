using Model.Web;

namespace UnitTests.Models;

[TestFixture]
public class SavedGameTests
{
    [Test]
    public void SavedGame_Constructor_ShouldSetAllProperties()
    {
        // Arrange
        var expectedId = "game123";
        var expectedName = "Adventure Game";
        var expectedDate = new DateTime(2023, 1, 15, 10, 30, 0);

        // Act
        var savedGame = new SavedGame(expectedId, expectedName, expectedDate);

        // Assert
        savedGame.Id.Should().Be(expectedId);
        savedGame.Name.Should().Be(expectedName);
        savedGame.Date.Should().Be(expectedDate);
    }

    [Test]
    public void SavedGame_Equals_WithIdenticalObjects_ShouldBeEqual()
    {
        // Arrange
        var date = new DateTime(2023, 1, 15);
        var savedGame1 = new SavedGame("game123", "Adventure Game", date);
        var savedGame2 = new SavedGame("game123", "Adventure Game", date);

        // Act & Assert
        savedGame1.Should().BeEquivalentTo(savedGame2);
        (savedGame1 == savedGame2).Should().BeTrue();
        savedGame1.Equals(savedGame2).Should().BeTrue();
    }

    [Test]
    public void SavedGame_Equals_WithDifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var date = new DateTime(2023, 1, 15);
        var savedGame1 = new SavedGame("game123", "Adventure Game", date);
        var savedGame2 = new SavedGame("game456", "Adventure Game", date);

        // Act & Assert
        savedGame1.Should().NotBeEquivalentTo(savedGame2);
    }

    [Test]
    public void SavedGame_Equals_WithDifferentName_ShouldNotBeEqual()
    {
        // Arrange
        var date = new DateTime(2023, 1, 15);
        var savedGame1 = new SavedGame("game123", "Adventure Game", date);
        var savedGame2 = new SavedGame("game123", "Different Game", date);

        // Act & Assert
        savedGame1.Should().NotBeEquivalentTo(savedGame2);
    }

    [Test]
    public void SavedGame_Equals_WithDifferentDate_ShouldNotBeEqual()
    {
        // Arrange
        var savedGame1 = new SavedGame("game123", "Adventure Game", new DateTime(2023, 1, 15));
        var savedGame2 = new SavedGame("game123", "Adventure Game", new DateTime(2023, 2, 20));

        // Act & Assert
        savedGame1.Should().NotBeEquivalentTo(savedGame2);
    }

    [Test]
    public void SavedGame_WithProperties_ShouldCreateValidInstance()
    {
        // Arrange
        var expectedId = "game123";
        var expectedName = "Adventure Game";
        var expectedDate = new DateTime(2023, 1, 15);

        // Act
        var savedGame = new SavedGame(expectedId, expectedName, expectedDate);

        // Assert
        savedGame.Id.Should().Be(expectedId);
        savedGame.Name.Should().Be(expectedName);
        savedGame.Date.Should().Be(expectedDate);
    }

    [Test]
    public void SavedGame_ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var savedGame = new SavedGame("game123", "Adventure Game", new DateTime(2023, 1, 15));

        // Act
        var result = savedGame.ToString();

        // Assert
        result.Should().Contain("game123");
        result.Should().Contain("Adventure Game");
        result.Should().Contain("2023");
    }

    [Test]
    public void SavedGame_WithOperator_ShouldCreateNewInstanceWithChangedProperty()
    {
        // Arrange
        var originalGame = new SavedGame("game123", "Adventure Game", new DateTime(2023, 1, 15));
        var newName = "Updated Game";

        // Act
        var updatedGame = originalGame with { Name = newName };

        // Assert
        updatedGame.Should().NotBeSameAs(originalGame);
        updatedGame.Id.Should().Be(originalGame.Id);
        updatedGame.Name.Should().Be(newName);
        updatedGame.Date.Should().Be(originalGame.Date);
        originalGame.Name.Should().Be("Adventure Game"); // Original unchanged
    }

    [Test]
    public void SavedGame_PropertySetters_ShouldOverrideConstructorValues()
    {
        // Arrange
        var initialId = "default";
        var initialName = "default";
        var initialDate = DateTime.MinValue;

        var expectedId = "game123";
        var expectedName = "Adventure Game";
        var expectedDate = new DateTime(2023, 1, 15);

        // Act
        var savedGame = new SavedGame(initialId, initialName, initialDate)
        {
            Id = expectedId,
            Name = expectedName,
            Date = expectedDate
        };

        // Assert
        savedGame.Id.Should().Be(expectedId);
        savedGame.Name.Should().Be(expectedName);
        savedGame.Date.Should().Be(expectedDate);
    }
}
