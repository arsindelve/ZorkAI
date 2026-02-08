namespace UnitTests;

[TestFixture]
public class ApplicableVerbsTests : EngineTestsBase
{
    [Test]
    public void SingleItemSingleVerb()
    {
        var engine = GetTarget();
        Take<Lunch>();
        engine.Context.GetAvailableActionsForInventory().Should().Contain("eat lunch");
    }

    [Test]
    public void SingleItemMultiVerb()
    {
        var engine = GetTarget();
        Take<Lunch>();
        engine.Context.GetAvailableActionsForInventory().Should().Contain("eat lunch");
        engine.Context.GetAvailableActionsForInventory().Should().Contain("drop lunch");
        engine.Context.GetAvailableActionsForInventory().Should().Contain("take lunch");
    }

    [Test]
    public void SingleItemMultiVerbInAttribute()
    {
        var engine = GetTarget();
        Take<Lantern>();
        engine.Context.GetAvailableActionsForInventory().Should().Contain("turn on lantern");
        engine.Context.GetAvailableActionsForInventory().Should().Contain("turn off lantern");
    }

    [Test]
    public void MultiItem()
    {
        var engine = GetTarget();
        Take<Lantern>();
        Take<Rope>();
        engine.Context.GetAvailableActionsForInventory().Should().Contain("turn on lantern");
        engine.Context.GetAvailableActionsForInventory().Should().Contain("drop rope");
    }
}