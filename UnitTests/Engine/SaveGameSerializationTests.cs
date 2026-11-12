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
    public async Task RestoreGame_RepopulatesPreviousLocationName_FromContext()
    {
        // Arrange: move the player so the engine records a "came from" location, then save.
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>();
        await engine.GetResponse("N");
        engine.LocationName.Should().Be("North of House");
        engine.PreviousLocationName.Should().Be("West Of House", "the player just came from West Of House");

        var saved = engine.SaveGame();

        // Act: restore into a fresh engine, exactly like the GET rehydrate path, which runs
        // no engine turn (so the engine-only PreviousLocationName is never re-assigned).
        var restoredEngine = GetTarget();
        restoredEngine.PreviousLocationName.Should().BeNull("a fresh engine has no prior location");
        restoredEngine.RestoreGame(saved);

        // Assert: PreviousLocationName must survive the no-turn GET rehydrate path so a
        // reconnecting client still knows where the player came from. It now lives on Context
        // (mirroring LastMovementDirection) so it serializes/restores and GET matches POST
        // (issue #250).
        restoredEngine.PreviousLocationName.Should().Be("West Of House");
    }

    [TestCase("ZorkOne")]
    [TestCase("Planetfall")]
    [TestCase("Stationfall")]
    [TestCase("EscapeRoom")]
    public void TheDiceAGameObjectRolls_AreNotWrittenIntoTheSavedGame(string game)
    {
        // A chooser is how an object behaves, not what state it is in. Written into a save it is dead
        // weight at best - and a save holding a stand-in chooser (a test's, or any type the serialiser
        // cannot rebuild) cannot be restored at all, which is how Cave South's was found.
        var written = System.Reflection.Assembly.Load(game).GetTypes()
            .SelectMany(type => type.GetProperties(System.Reflection.BindingFlags.Public |
                                                   System.Reflection.BindingFlags.Instance |
                                                   System.Reflection.BindingFlags.DeclaredOnly))
            .Where(property => property.PropertyType == typeof(Model.Interface.IRandomChooser) &&
                               !property.IsDefined(typeof(Newtonsoft.Json.JsonIgnoreAttribute), false))
            .Select(property => $"{property.DeclaringType!.Name}.{property.Name}");

        written.Should().BeEmpty("every IRandomChooser property carries [JsonIgnore]");
    }

    [Test]
    public void AGameSavedInCaveSouth_WithItsDicePinned_CanBeRestored()
    {
        var engine = GetTarget();
        var pinned = new Mock<Model.Interface.IRandomChooser>();
        Repository.GetLocation<CaveSouth>().RandomChooser = pinned.Object;

        var saved = engine.SaveGame();

        var restoring = () => GetTarget().RestoreGame(saved);
        restoring.Should().NotThrow();
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
