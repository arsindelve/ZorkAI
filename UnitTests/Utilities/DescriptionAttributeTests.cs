using System.Reflection;
using DescriptionAttribute = Utilities.DescriptionAttribute;

namespace UnitTests.Utilities;

[TestFixture]
public class DescriptionAttributeTests
{
    private enum TestEnum
    {
        [Description("First value description")]
        FirstValue,

        [Description("Second value description")]
        SecondValue,

        ThirdValueWithoutDescription
    }

    [Test]
    public void Constructor_ShouldSetDescriptionProperty()
    {
        // Arrange & Act
        var description = "Test description";
        var attribute = new DescriptionAttribute(description);

        // Assert
        attribute.Description.Should().Be(description);
    }

    [Test]
    public void Description_ShouldBeReadOnly()
    {
        // Arrange
        var description = "Test description";
        var attribute = new DescriptionAttribute(description);
        var property = typeof(DescriptionAttribute).GetProperty("Description");

        // Assert
        property.Should().NotBeNull();
        property!.CanRead.Should().BeTrue();
        property.CanWrite.Should().BeFalse();
    }

    [Test]
    public void AttributeUsage_ShouldBeRestrictedToFields()
    {
        // Arrange
        var attributeUsage = typeof(DescriptionAttribute).GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        attributeUsage.Should().NotBeNull();
        attributeUsage!.ValidOn.Should().Be(AttributeTargets.Field);
    }

    [Test]
    public void GetDescription_FromEnumField_ShouldReturnCorrectDescription()
    {
        // Arrange
        var firstValueField = typeof(TestEnum).GetField(nameof(TestEnum.FirstValue));
        var secondValueField = typeof(TestEnum).GetField(nameof(TestEnum.SecondValue));

        // Act
        var firstValueAttr = firstValueField!.GetCustomAttribute<DescriptionAttribute>();
        var secondValueAttr = secondValueField!.GetCustomAttribute<DescriptionAttribute>();

        // Assert
        firstValueAttr.Should().NotBeNull();
        firstValueAttr!.Description.Should().Be("First value description");

        secondValueAttr.Should().NotBeNull();
        secondValueAttr!.Description.Should().Be("Second value description");
    }

    [Test]
    public void GetDescription_FromEnumFieldWithoutDescription_ShouldReturnNull()
    {
        // Arrange
        var field = typeof(TestEnum).GetField(nameof(TestEnum.ThirdValueWithoutDescription));

        // Act
        var attribute = field!.GetCustomAttribute<DescriptionAttribute>();

        // Assert
        attribute.Should().BeNull();
    }

    [Test]
    public void GetEnumValueWithDescription_UsingReflection()
    {
        // Arrange & Act
        var descriptionValues = new Dictionary<TestEnum, string>();

        foreach (TestEnum value in Enum.GetValues(typeof(TestEnum)))
        {
            var fieldInfo = typeof(TestEnum).GetField(value.ToString());
            var attribute = fieldInfo!.GetCustomAttribute<DescriptionAttribute>();

            if (attribute != null) descriptionValues.Add(value, attribute.Description);
        }

        // Assert
        descriptionValues.Count.Should().Be(2);
        descriptionValues[TestEnum.FirstValue].Should().Be("First value description");
        descriptionValues[TestEnum.SecondValue].Should().Be("Second value description");
        descriptionValues.ContainsKey(TestEnum.ThirdValueWithoutDescription).Should().BeFalse();
    }
}