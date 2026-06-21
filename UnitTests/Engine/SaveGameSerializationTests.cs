using GameEngine;
using Newtonsoft.Json.Linq;

namespace UnitTests.Engine;

public class SaveGameSerializationTests : EngineTestsBase
{
    [Test]
    public async Task RestoreGame_RepopulatesInventoryProjection_FromContextItems()
    {
        // Arrange: progress a game so the player is holding the lantern, then save it.
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        await engine.GetResponse("take lantern");
        engine.Inventory.Should().Contain("lantern", "the lantern is now held");

        var saved = engine.SaveGame();

        // Act: restore into a fresh engine, exactly like the GET rehydrate path, which runs
        // no engine turn. A fresh engine is holding nothing.
        var restoredEngine = GetTarget();
        restoredEngine.Inventory.Should().BeEmpty("a fresh engine is holding nothing");
        restoredEngine.RestoreGame(saved);

        // Assert: the flat Inventory projection must reflect the restored Context.Items even
        // without a turn, otherwise a returning player sees empty hands on reconnect/refresh
        // (issue #230). Inventory is now derived live from Context.Items so it cannot drift.
        restoredEngine.Context.HasItem<Lantern>().Should().BeTrue("the saved state holds the lantern");
        restoredEngine.Inventory.Should().Contain("lantern");
    }

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
