namespace UnitTests;

[TestFixture]
public class ApplicableVerbsTests : EngineTestsBase
{
    [Test]
    public void Context_SingleItemSingleVerb()
    {
        var engine = GetTarget();
        Take<Lunch>();
        engine.Context.GetAvailableActionsForInventory().SelectMany(s => s.Value).Should().Contain("eat lunch");
    }

    [Test]
    public void Context_SingleItemMultiVerb()
    {
        var engine = GetTarget();
        Take<Lunch>();
        engine.Context.GetAvailableActionsForInventory().SelectMany(s => s.Value).Should().Contain("eat lunch");
        engine.Context.GetAvailableActionsForInventory().SelectMany(s => s.Value).Should().Contain("drop lunch");
        engine.Context.GetAvailableActionsForInventory().SelectMany(s => s.Value).Should().NotContain("take lunch");
    }

    [Test]
    public void Context_SingleItemMultiVerbInAttribute()
    {
        var engine = GetTarget();
        Take<Lantern>();
        engine.Context.GetAvailableActionsForInventory().SelectMany(s => s.Value).Should().Contain("turn on lantern");
        engine.Context.GetAvailableActionsForInventory().SelectMany(s => s.Value).Should().Contain("turn off lantern");
    }

    [Test]
    public void Context_MultiItem()
    {
        var engine = GetTarget();
        Take<Lantern>();
        Take<Rope>();
        engine.Context.GetAvailableActionsForInventory().SelectMany(s => s.Value).Should().Contain("turn on lantern");
        engine.Context.GetAvailableActionsForInventory().SelectMany(s => s.Value).Should().Contain("drop rope");
    }

    [Test]
    public void Location()
    {
        var engine = GetTarget();
        StartHere<MaintenanceRoom>();
        var selectMany = engine.Context.CurrentLocation.GetAvailableActionsInLocation();
        var result = selectMany.SelectMany(s => s.Value).ToList();
        result.Should().Contain("examine tool chest");
        result.Should().Contain("take tube");
        result.Should().NotContain("drop tube");
    }
}