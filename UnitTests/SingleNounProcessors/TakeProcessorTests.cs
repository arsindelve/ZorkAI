using GameEngine;
using GameEngine.Item.ItemProcessor;
using Model.AIGeneration;
using Model.Intent;
using Model.Interaction;
using Model.Interface;

namespace UnitTests.SingleNounProcessors;

public class TakeProcessorTests : EngineTestsBase
{
    [Test]
    public async Task Take_ItemInsideOpenItem()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("open sack");
        var result = await target.GetResponse("take lunch");

        // Assert
        result.Should().Contain("Taken");
    }

    [Test]
    public async Task Take_ItemInsideClosedItem()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        var result = await target.GetResponse("take lunch");

        // Assert
        result.Should().NotContain("Taken");
    }

    [Test]
    public void CannotBeTaken_WrongType()
    {
        IVerbProcessor target = new CannotBeTakenProcessor();
        Assert.Throws<Exception>(() => target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new MadeUpItem(),
            Mock.Of<IGenerationClient>()));
    }

    [Test]
    public async Task CannotBeTaken_PositiveInteraction()
    {
        var target = GetTarget();

        var result = await target.GetResponse("take mailbox");

        result.Should().Contain("securely");
    }

    [Test]
    public async Task CannotBeTaken_GetVerb_PositiveInteraction()
    {
        // Issue #406: "get" must behave exactly like "take" on a non-takeable item. It used to fall
        // through to the improvised "verb has no effect" narration instead of the authored refusal.
        var target = GetTarget();

        var result = await target.GetResponse("get mailbox");

        result.Should().Contain("securely");
    }

    [TestCaseSource(nameof(AllTakeVerbs))]
    public async Task CannotBeTaken_CoversTheWholeTakeVerbFamily(string verb)
    {
        // Issue #406: CannotBeTakenProcessor kept its own hardcoded copy of the take verbs, which
        // had drifted from Verbs.TakeVerbs ("get" and "grab" were missing). Every verb the engine
        // treats as a take must surface the item's authored CannotBeTakenDescription, just as
        // TakeOrDropInteractionProcessor does for takeable items. Sourcing the cases from
        // Verbs.TakeVerbs keeps this contract from drifting again when a synonym is added.
        var target = GetTarget();

        IVerbProcessor processor = new CannotBeTakenProcessor();
        var result = await processor.Process(
            new SimpleIntent { Verb = verb, Noun = "mailbox", OriginalInput = $"{verb} mailbox" },
            target.Context, Repository.GetItem<Mailbox>(), Client.Object);

        result.Should().BeOfType<PositiveInteractionResult>(
            $"'{verb}' is in Verbs.TakeVerbs, so it must trigger the authored refusal");
        result!.InteractionMessage.Should().Contain("securely anchored");
    }

    private static IEnumerable<string> AllTakeVerbs => Verbs.TakeVerbs;

    [Test]
    public async Task TakeSecondItemFromContainer()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        await target.GetResponse("open sack");
        var result = await target.GetResponse("take garlic");
        result.Should().Contain("Taken");
    }

    [Test]
    public async Task TakeItemFromClosedTransparentContainer()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        Repository.GetItem<TrophyCase>().ItemPlacedHere(Repository.GetItem<Torch>());
        Repository.GetItem<Torch>().CurrentLocation = Repository.GetItem<TrophyCase>();

        var result = await target.GetResponse("take torch");
        result.Should().Contain("closed container");
    }

    [Test]
    public async Task TakeFirstItemFromContainer()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        await target.GetResponse("open sack");
        var result = await target.GetResponse("take lunch");
        result.Should().Contain("Taken");
    }

    [Test]
    public async Task DropItemIDoNotHave()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        var result = await target.GetResponse("drop rope");
        result.Should().Contain("don't have that");
    }

    [Test]
    public async Task TakeItemIAlreadyHave()
    {
        var target = GetTarget();
        Repository.GetItem<Lantern>().IsOn = true;
        target.Context.Take(Repository.GetItem<Lantern>());
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();
        await target.GetResponse("take rope");
        var result = await target.GetResponse("take rope");
        result.Should().Contain("already have that");
    }

    [Test]
    public async Task TakeItemIAlreadyHaveInsideAContainer()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        await target.GetResponse("take sack");
        await target.GetResponse("open sack");
        var result = await target.GetResponse("take garlic");
        result.Should().Contain("Taken");
    }

    [Test]
    public async Task TakeItem_InDarkRoom_ViaTakeIntent_SaysTooDarkAndDoesNotTakeIt()
    {
        // Issue #342: production's real AI parser tags a bare "take rope" as a TakeIntent, which
        // GameEngine dispatches straight to TakeOrDropInteractionProcessor.Process(TakeIntent, ...),
        // bypassing the darkness guard that SimpleIntent goes through in SimpleInteractionEngine.
        // TestParser (used by every other test in this file via GetResponse) always resolves "take X"
        // to a SimpleIntent, so it can't reproduce this - the TakeIntent-facing overload has to be
        // invoked directly, exactly as GameEngine.cs does for a live AI-tagged "take" intent.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();

        target.Context.ItIsDarkHere.Should().BeTrue();

        var processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var (_, message) = await processor.Process(
            new TakeIntent { Noun = "rope", OriginalInput = "take rope" }, target.Context, Client.Object);

        message.Should().Contain("too dark");
        target.Context.HasItem<Rope>().Should().BeFalse();
    }

    [Test]
    public async Task TakeItem_ViaTakeIntent_SucceedsAfterRelightingLantern()
    {
        // Control for the test above: once there's light again, the same TakeIntent path should
        // still let the player take the rope.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        target.Context.ItIsDarkHere.Should().BeFalse();

        var processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var (_, message) = await processor.Process(
            new TakeIntent { Noun = "rope", OriginalInput = "take rope" }, target.Context, Client.Object);

        message.Should().Contain("Taken");
        target.Context.HasItem<Rope>().Should().BeTrue();
    }

    [Test]
    public async Task TakeAll_InDarkRoom_SaysTooDarkAndDoesNotTakeAnything()
    {
        // PR review follow-up to issue #342: the plain "take all"/"take everything" global command
        // (GlobalCommandFactory -> TakeEverythingProcessor.Process) is matched before the AI parser
        // ever runs, so none of the TakeIntent/SimpleIntent darkness guards apply to it. TestParser
        // doesn't override global-command matching, so GetResponse("take all") exercises the exact
        // same real dispatch path production uses here.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();

        target.Context.ItIsDarkHere.Should().BeTrue();

        var result = await target.GetResponse("take all");

        result.Should().Contain("too dark");
        target.Context.HasItem<Rope>().Should().BeFalse();
        target.Context.HasItem<NastyKnife>().Should().BeFalse();
    }

    [Test]
    public async Task TakeMultipleItems_InDarkRoom_ViaTakeIntent_DoesNotTakeAnyOfThem()
    {
        // PR review follow-up to issue #342: a live AI TakeIntent can resolve more than one noun for
        // a single command (e.g. "take rope and knife"), which GetItemsToTake routes to
        // TakeEverythingProcessor.TakeAll instead of TakeIt - a separate branch that had its own,
        // still-unguarded darkness gap even after the single-item TakeIt fix above.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();

        target.Context.ItIsDarkHere.Should().BeTrue();

        var processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var (_, message) = await processor.Process(
            new TakeIntent { Noun = "rope", OriginalInput = "take rope and knife" }, target.Context, Client.Object);

        message.Should().NotContain("Taken");
        message.Should().Contain("too dark");
        target.Context.HasItem<Rope>().Should().BeFalse();
        target.Context.HasItem<NastyKnife>().Should().BeFalse();
    }

    [Test]
    public async Task TakeItem_Disambiguation()
    {
        var target = GetTarget();
        var location = Repository.GetLocation<Kitchen>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<NastyKnife>());
        location.ItemPlacedHere(Repository.GetItem<RustyKnife>());

        target.Context.HasItem<NastyKnife>().Should().BeFalse();
        target.Context.HasItem<RustyKnife>().Should().BeFalse();

        await target.GetResponse("take knife");
        var result = await target.GetResponse("rusty");

        result.Should().Contain("Taken");
        target.Context.HasItem<RustyKnife>().Should().BeTrue();

        await target.GetResponse("take knife");
        await target.GetResponse("nasty");

        result.Should().Contain("Taken");
        target.Context.HasItem<NastyKnife>().Should().BeTrue();
    }
}