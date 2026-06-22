using GameEngine;

namespace UnitTests.GlobalCommands;

public class GodModeTests : EngineTestsBase
{
    // Issue #241 follow-up: LoadAllLocations now Init()s every newly-created location, so the
    // explicit location.Init() that GodModeProcessor.Go() also performed ran Init() twice. For any
    // location using StartWithItem<T> (a bare Items.Add with no dedupe, e.g. the Kitchen's window,
    // sack and bottle), that double-Init() duplicated each starting item in the room. "god mode go"
    // must leave each starting item in the room exactly once.
    [Test]
    public async Task GodModeGo_DoesNotDuplicateStartingItems()
    {
        var engine = GetTarget();

        await engine.GetResponse("god mode go kitchen");

        var kitchen = Repository.GetLocation<Kitchen>();
        kitchen.Items.Should().OnlyHaveUniqueItems();
    }
}
