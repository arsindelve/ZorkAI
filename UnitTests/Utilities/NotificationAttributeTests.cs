using Utilities;

namespace UnitTests.Utilities;

[TestFixture]
public class NotificationAttributeTests
{
    [Test]
    public void Constructor_ShouldSetDescriptionProperty()
    {
        // Arrange
        const string expectedDescription = "This is a test notification";

        // Act
        var attribute = new NotificationAttribute(expectedDescription);

        // Assert
        attribute.Description.Should().Be(expectedDescription);
    }

    [Test]
    public void Constructor_WithEmptyString_ShouldSetEmptyDescription()
    {
        // Arrange
        const string expectedDescription = "";

        // Act
        var attribute = new NotificationAttribute(expectedDescription);

        // Assert
        attribute.Description.Should().Be(expectedDescription);
    }

    [Test]
    public void AttributeUsage_ShouldBeRestrictedToFields()
    {
        // Arrange & Act
        var attributeUsage = Attribute.GetCustomAttribute(
            typeof(NotificationAttribute),
            typeof(AttributeUsageAttribute)) as AttributeUsageAttribute;

        // Assert
        attributeUsage.Should().NotBeNull();
        attributeUsage!.ValidOn.Should().Be(AttributeTargets.Field);
    }

    [Test]
    public void Description_ShouldBeReadOnly()
    {
        // Arrange
        _ = new NotificationAttribute("Test");

        // Act & Assert
        var property = typeof(NotificationAttribute).GetProperty("Description");
        property.Should().NotBeNull();
        property!.CanRead.Should().BeTrue();
        property.CanWrite.Should().BeFalse();
    }
}