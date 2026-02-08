namespace UnitTests;

[TestFixture]
public class ApplicableVerbsTests : EngineTestsBase
{
    [Test]
    public void Context_SingleItemSingleVerb()
    {
        var engine = GetTarget();
        Take<Lunch>();
        engine.Context.GetAvailableActionsForInventory().Should().Contain("eat lunch");
    }

    [Test]
    public void Context_SingleItemMultiVerb()
    {
        var engine = GetTarget();
        Take<Lunch>();
        engine.Context.GetAvailableActionsForInventory().Should().Contain("eat lunch");
        engine.Context.GetAvailableActionsForInventory().Should().Contain("drop lunch");
        engine.Context.GetAvailableActionsForInventory().Should().Contain("take lunch");
    }

    [Test]
    public void Context_SingleItemMultiVerbInAttribute()
    {
        var engine = GetTarget();
        Take<Lantern>();
        engine.Context.GetAvailableActionsForInventory().Should().Contain("turn on lantern");
        engine.Context.GetAvailableActionsForInventory().Should().Contain("turn off lantern");
    }

    [Test]
    public void Context_MultiItem()
    {
        var engine = GetTarget();
        Take<Lantern>();
        Take<Rope>();
        engine.Context.GetAvailableActionsForInventory().Should().Contain("turn on lantern");
        engine.Context.GetAvailableActionsForInventory().Should().Contain("drop rope");
    }

    [Test]
    public void Location()
    {
        var engine = GetTarget();
        StartHere<MaintenanceRoom>();
        engine.Context.CurrentLocation.GetAvailableActionsInLocation().Should().Contain("examine tool chest");
        engine.Context.CurrentLocation.GetAvailableActionsInLocation().Should().Contain("take tube");
    }
}