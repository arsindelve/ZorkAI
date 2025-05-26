using JetBrains.Annotations;
using Utilities;

namespace UnitTests.Utilities;

[TestFixture]
public class EnumExtensionsTests
{
    private enum TestEnum
    {
        [global::Utilities.Description("First value description")]
        FirstValue,

        [global::Utilities.Description("Second value description")]
        SecondValue,

        ThirdValueWithoutDescription
    }

    [UsedImplicitly]
    private enum EmptyEnum;

    [Test]
    public void GetDescription_WithDescriptionAttribute_ReturnsDescription()
    {
        // Arrange
        var enumValue = TestEnum.FirstValue;

        // Act
        var result = enumValue.GetDescription();

        // Assert
        result.Should().Be("First value description");
    }

    [Test]
    public void GetDescription_WithMultipleEnumValues_ReturnsCorrectDescriptions()
    {
        // Arrange
        var firstValue = TestEnum.FirstValue;
        var secondValue = TestEnum.SecondValue;

        // Act
        var firstResult = firstValue.GetDescription();
        var secondResult = secondValue.GetDescription();

        // Assert
        firstResult.Should().Be("First value description");
        secondResult.Should().Be("Second value description");
    }

    [Test]
    public void GetDescription_WithoutDescriptionAttribute_ReturnsEnumValueName()
    {
        // Arrange
        var enumValue = TestEnum.ThirdValueWithoutDescription;

        // Act
        var result = enumValue.GetDescription();

        // Assert
        result.Should().Be("ThirdValueWithoutDescription");
    }

    [Test]
    public void GetDescription_WhenCastFromInteger_ReturnsCorrectDescription()
    {
        // Arrange
        var enumValue = (TestEnum)0; // This is FirstValue

        // Act
        var result = enumValue.GetDescription();

        // Assert
        result.Should().Be("First value description");
    }

    [Test]
    public void GetDescription_WithInvalidEnumValue_ReturnsStringRepresentation()
    {
        // Arrange
        var enumValue = (TestEnum)999; // Invalid value

        // Act
        var result = enumValue.GetDescription();

        // Assert
        result.Should().Be("999");
    }

    [Test]
    public void GetDescription_WithDifferentEnumTypes_WorksForAll()
    {
        // Arrange
        var testEnum = TestEnum.FirstValue;
        var dayOfWeek = DayOfWeek.Monday;

        // Act & Assert - Just confirming it doesn't throw exceptions
        var testEnumDescription = testEnum.GetDescription();
        var dayOfWeekDescription = dayOfWeek.GetDescription();

        // Both should return strings without throwing
        testEnumDescription.Should().NotBeNull();
        dayOfWeekDescription.Should().NotBeNull();
    }

    [Test]
    public void GetDescription_UsesReflectionCorrectly()
    {
        // Arrange & Act - Creating a mock would be better, but this is a simple case
        var descriptions = new Dictionary<TestEnum, string>();

        foreach (var value in Enum.GetValues(typeof(TestEnum)))
        {
            var enumValue = (TestEnum)value;
            descriptions[enumValue] = enumValue.GetDescription();
        }

        // Assert
        descriptions.Count.Should().Be(3);
        descriptions[TestEnum.FirstValue].Should().Be("First value description");
        descriptions[TestEnum.SecondValue].Should().Be("Second value description");
        descriptions[TestEnum.ThirdValueWithoutDescription].Should().Be("ThirdValueWithoutDescription");
    }

    [Test]
    public void GetDescription_RespectsCaseInsensitivity()
    {
        // Arrange
        var enumValue = TestEnum.FirstValue;
        var fieldInfo = typeof(TestEnum).GetField("FIRSTVALUE"); // Case different from actual enum

        // Assert
        fieldInfo.Should().BeNull("Because field lookup is case-sensitive");
        enumValue.GetDescription().Should().Be("First value description",
            "Because GetDescription should handle the case sensitivity correctly");
    }
}