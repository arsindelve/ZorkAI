using GameEngine;
using Newtonsoft.Json.Linq;

namespace UnitTests.Engine;

public class SaveGameSerializationTests : EngineTestsBase
{
    [Test]
    public void SaveGame_Contains_TypeAndReferenceMetadata()
    {
        // Arrange
        var engine = GetTarget();

        // Act
        var json = engine.SaveGame();

        // Assert: Type and reference metadata should be present due to serializer settings
        json.Should().Contain("\"$type\"");
        (json.Contains("\"$id\"") || json.Contains("\"$ref\""))
            .Should().BeTrue("PreserveReferencesHandling.Objects should include $id/$ref markers");

        // Optional shape checks for main sections
        var j = JObject.Parse(json);
        j.Should().ContainKey("AllItems");
        j.Should().ContainKey("AllLocations");
        j.Should().ContainKey("Context");
        j["Context"]!.Should().NotBeNull();
        j["Context"]!.ToString().Should().Contain("CurrentLocation");
    }
}
