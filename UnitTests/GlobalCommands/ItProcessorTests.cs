using GameEngine;
using Model.AIGeneration.Requests;

namespace UnitTests.GlobalCommands;

public class ItProcessorTests : EngineTestsBase
{
    [Test]
    public async Task It_SuccessPath()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");
        var result = await target.GetResponse("drop it");

        target.Context.HasItem<Lantern>().Should().BeFalse();
        result.Should().Contain("Dropped");
    }

    [Test]
    public async Task Them_SuccessPath()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Altar>();

        await target.GetResponse("take candles");
        var result = await target.GetResponse("drop them");

        target.Context.HasItem<Candles>().Should().BeFalse();
        result.Should().Contain("Dropped");
    }

    [Test]
    public async Task Them_WithItemThatIsNotPlural()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");
        var result = await target.GetResponse("drop them");

        target.Context.HasItem<Lantern>().Should().BeTrue();
        result.Should().Contain("What item are you referring to");
    }

    [Test]
    public async Task Them_WithItemThatIsNotPlural_Disambiguation()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");
        var result = await target.GetResponse("drop them");

        target.Context.HasItem<Lantern>().Should().BeTrue();
        result.Should().Contain("What item are you referring to");

        result = await target.GetResponse("lantern");

        target.Context.HasItem<Candles>().Should().BeFalse();
        result.Should().Contain("Dropped");
    }

    [Test]
    public async Task It_NoLastNoun()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        var result = await target.GetResponse("take it");
        result = await target.GetResponse("lantern");

        target.Context.HasItem<Lantern>().Should().BeTrue();
        result.Should().Contain("Taken");
    }

    // Issue #248: "them" must resolve to a *collection* of items, not just a single
    // intrinsically-plural item (like candles). After "take all" the player should be
    // able to say "drop them" and have everything they just took be dropped.
    [Test]
    public async Task Them_AfterTakeAll_DropsEverythingTaken()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take all");
        target.Context.HasItem<Lantern>().Should().BeTrue();
        target.Context.HasItem<Sword>().Should().BeTrue();

        var result = await target.GetResponse("drop them");

        result.Should().NotContain("What item are you referring to");
        result.Should().Contain("Dropped");
        target.Context.HasItem<Lantern>().Should().BeFalse();
        target.Context.HasItem<Sword>().Should().BeFalse();
    }

    // Issue #248: a collection built up from several individual takes should also be
    // addressable as "them".
    [Test]
    public async Task Them_AfterSeveralIndividualTakes_DropsAllOfThem()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");
        await target.GetResponse("take sword");

        var result = await target.GetResponse("drop them");

        result.Should().NotContain("What item are you referring to");
        result.Should().Contain("Dropped");
        target.Context.HasItem<Lantern>().Should().BeFalse();
        target.Context.HasItem<Sword>().Should().BeFalse();
    }

    // Issue #248 (review follow-up): "them" must only resolve to members still relevant to the
    // command. An item dropped between takes is no longer held, so "drop them" should drop only what
    // is still in hand — not drag the already-dropped item back in and trigger a spurious
    // "you don't have it" narration.
    [Test]
    public async Task Them_DoesNotReferToItemsNoLongerInScope()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");
        await target.GetResponse("take sword");
        await target.GetResponse("drop lantern"); // lantern leaves inventory, but was remembered

        var result = await target.GetResponse("drop them");

        result.Should().Contain("Dropped");
        target.Context.HasItem<Sword>().Should().BeFalse();
        // No AI narration should fire for the stale lantern that is no longer held.
        Client.Verify(c => c.GenerateNarration(
            It.IsAny<DropSomethingTheyDoNotHave>(), It.IsAny<string>()), Times.Never);
    }
}