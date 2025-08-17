using Newtonsoft.Json;
using GameEngine;

namespace UnitTests.Utilities;

public class SerializationResolverTests
{
    private class WithReadOnly
    {
        public int Writable { get; set; } = 42;
        public int ReadOnly { get; } = 7; // no setter
    }

    [Test]
    public void ReadOnlyProperties_AreExcluded_ByResolver()
    {
        // Arrange
        var obj = new WithReadOnly();
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DoNotSerializeReadOnlyPropertiesResolver(),
            Formatting = Formatting.None
        };

        // Act
        var json = JsonConvert.SerializeObject(obj, settings);

        // Assert
        json.Should().Contain("Writable");
        json.Should().NotContain("ReadOnly");
    }
}
