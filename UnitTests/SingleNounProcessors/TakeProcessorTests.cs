using GameEngine;
using GameEngine.Item;
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

    [TestCase("take mailbox")]
    [TestCase("get mailbox")]
    public async Task CannotBeTaken_PositiveInteraction(string input)
    {
        // Issue #406: "get" must behave exactly like "take" on a non-takeable item. It used to fall
        // through to the improvised "verb has no effect" narration instead of the authored refusal.
        var target = GetTarget();

        var result = await target.GetResponse(input);

        result.Should().Contain("securely");
    }

    [TestCase("take")]
    [TestCase("get")]
    [TestCase("grab")]
    [TestCase("pick up")]
    [TestCase("hold")]
    [TestCase("acquire")]
    [TestCase("snatch")]
    [TestCase("carry")]
    public async Task CannotBeTaken_CoversTheWholeTakeVerbFamily(string verb)
    {
        // Issue #406: CannotBeTakenProcessor kept its own hardcoded copy of the take verbs, which
        // had drifted from Verbs.TakeVerbs ("get" and "grab" were missing). Every verb the engine
        // treats as a take must surface the item's authored CannotBeTakenDescription, just as
        // TakeOrDropInteractionProcessor does for takeable items. The cases are deliberately
        // literal, not sourced from Verbs.TakeVerbs: a self-referential source would shrink in
        // lockstep if a synonym were ever removed from the family, hiding the regression. "carry"
        // is a canonical original synonym (planetfall-source syntax.zil:334
        // <SYNONYM TAKE GET HOLD CARRY>).
        var target = GetTarget();

        IVerbProcessor processor = new CannotBeTakenProcessor();
        var result = await processor.Process(
            new SimpleIntent { Verb = verb, Noun = "mailbox", OriginalInput = $"{verb} mailbox" },
            target.Context, Repository.GetItem<Mailbox>(), Client.Object);

        result.Should().BeOfType<PositiveInteractionResult>(
            $"'{verb}' is in Verbs.TakeVerbs, so it must trigger the authored refusal");
        result!.InteractionMessage.Should().Contain("securely anchored");
    }

    [Test]
    public async Task CannotBeTaken_FiresTheOnFailingToBeTakenHook()
    {
        // The TakeIntent refusal branch (TakeOrDropInteractionProcessor.TakeIt) invokes
        // OnFailingToBeTaken before returning CannotBeTakenDescription. This SimpleIntent branch
        // must do the same, or a failed take's side effects (the Slag/ToolChests
        // destroy-on-failed-take seam) would depend on which parse path delivered the verb.
        var target = GetTarget();
        var relic = new AnchoredRelic();

        IVerbProcessor processor = new CannotBeTakenProcessor();
        var result = await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "relic", OriginalInput = "take relic" },
            target.Context, relic, Client.Object);

        result!.InteractionMessage.Should().Contain("fused to its pedestal");
        relic.FailedTakeCount.Should().Be(1, "the refusal must fire the same hook the TakeIntent path fires");
    }

    private class AnchoredRelic : ItemBase
    {
        public int FailedTakeCount { get; private set; }

        public override string[] NounsForMatching => ["relic"];

        public override string? CannotBeTakenDescription
        {
            get => "The relic is fused to its pedestal. ";
            set { }
        }

        public override void OnFailingToBeTaken(IContext context)
        {
            FailedTakeCount++;
        }
    }

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
    public async Task Take_NounMatchesNothingInScope_DoesNotSilentlyTakeTheRoomsOnlyItem()
    {
        // Issue #502: the AI take-list parser is scoped to the room description, so a room holding
        // exactly one takeable item makes it return that item for ANY noun - there is nothing else it
        // could name. The engine then treated "the parser returned one thing" as "the player asked for
        // that thing" and answered a bare "Taken.", silently pocketing an object the player never
        // named. The stub below is what production really does here; only a guard on the player's own
        // noun can tell the two apart.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Lantern>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["lantern"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "xyzzy", OriginalInput = "take xyzzy" },
            target.Context, Repository.GetItem<Lantern>(), Client.Object);

        result.Should().BeOfType<NoNounMatchInteractionResult>();
        target.Context.HasItem<Lantern>().Should().BeFalse("the player never named the lantern");
    }

    [Test]
    public async Task Take_NounNamesADifferentInScopeItem_TakesTheOneThePlayerNamed()
    {
        // Issue #502, the misleading variant: the parser only ever sees the room description, so when
        // the player names something they are carrying (the reported case was "take medicine" with the
        // medicine inside a held bottle) it still answers with the room's lone item. The player's noun
        // resolves perfectly well - it just isn't the thing the parser returned - so the take must
        // follow the noun, not the parser.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Lantern>());

        var sack = Repository.GetItem<BrownSack>();
        target.Context.Take(sack);
        sack.IsOpen = true;

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["lantern"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "lunch", OriginalInput = "take lunch" },
            target.Context, Repository.GetItem<Lantern>(), Client.Object);

        result!.InteractionMessage.Should().Contain("Taken");
        target.Context.HasItem<Lunch>().Should().BeTrue("the player asked for the lunch");
        target.Context.HasItem<Lantern>().Should().BeFalse("the lantern was never named");
    }

    [Test]
    public async Task Take_CompoundPhraseFromTheParser_StillResolvesToThePlayersNoun()
    {
        // Issue #502 guard rail: the parser routinely returns an adjective-qualified phrase ("brass
        // lantern") for a bare noun ("lantern"). That is the same object, so the noun check must not
        // reject it - this is exactly the case the pre-existing fallback comment protects.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Lantern>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["brass lantern"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "lantern", OriginalInput = "take lantern" },
            target.Context, Repository.GetItem<Lantern>(), Client.Object);

        result!.InteractionMessage.Should().Contain("Taken");
        target.Context.HasItem<Lantern>().Should().BeTrue();
    }

    [Test]
    public async Task Take_TheItemThePlayerActuallyNamed_IsStillTaken()
    {
        // Issue #502 guard rail: the ordinary, overwhelmingly common case must be untouched.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Lantern>());

        var result = await target.GetResponse("take lantern");

        result.Should().Contain("Taken");
        target.Context.HasItem<Lantern>().Should().BeTrue();
    }

    [Test]
    public async Task Take_BareVerbWithTwoItemsPresent_StillAsksWhichOne()
    {
        // Issue #502 guard rail: the bare-"take" convenience is a different processor
        // (TakeOnlyAvailableItemProcessor) and must keep its disambiguation prompt.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Lantern>());
        location.ItemPlacedHere(Repository.GetItem<Sword>());

        var result = await target.GetResponse("take");

        result.Should().Contain("What do you want to take?");
    }

    [Test]
    public async Task Take_AdjectiveTheParserDidNotEcho_StillMatchesAContainerItem()
    {
        // Issue #502 guard rail: the noun check must not be shadowed by the container overrides.
        // ItemBase.HasMatchingNoun has a word-boundary containment fallback, but ContainerBase and
        // OpenAndCloseContainerBase override it with plain exact equality - so for every
        // container-derived item (sack, bottle, canteen, coffin, survival kit...) an adjective the
        // player added but the parser didn't echo would look like a mismatch and be refused.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<BrownSack>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["sack"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent
                { Verb = "take", Noun = "elongated sack", OriginalInput = "take the elongated sack" },
            target.Context, Repository.GetItem<BrownSack>(), Client.Object);

        result!.InteractionMessage.Should().Contain("Taken");
        target.Context.HasItem<BrownSack>().Should().BeTrue();
    }

    [Test]
    public async Task Take_QuantifiedRequest_StillTakesTheOneQualifyingItem()
    {
        // Issue #502 guard rail: TakeIntent.Noun is only nouns.FirstOrDefault() (ParsingHelper), so
        // for a quantified command it is a collective word or - as here - the *excluded* noun, never
        // the object the parser resolved. Applying the noun check to those would not just refuse the
        // take, it would actively take the wrong thing (the sword the player explicitly excluded).
        // "pick up everything except the tube" is a supported phrasing - see
        // IntegrationTests/OpenAITakeAndDropParserTests.cs.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Lantern>());
        location.ItemPlacedHere(Repository.GetItem<Sword>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["lantern"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent
            {
                Verb = "take", Noun = "sword",
                OriginalInput = "pick up everything except the sword"
            },
            target.Context, Repository.GetItem<Lantern>(), Client.Object);

        result!.InteractionMessage.Should().Contain("Taken");
        target.Context.HasItem<Lantern>().Should().BeTrue("it is the only item the quantifier covers");
        target.Context.HasItem<Sword>().Should().BeFalse("the player explicitly excluded the sword");
    }

    [Test]
    public async Task Take_MultipleObjectsNamedButOnlyOnePresent_StillTakesThePresentOne()
    {
        // Issue #502 guard rail: on "take X and Y" the parser only ever returns what the room
        // actually holds, while action.Noun is whichever noun came first. A single returned candidate
        // is then a legitimate partial resolution of a multi-object request, not a substitution.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Lantern>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["lantern"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "sword", OriginalInput = "take the sword and the lantern" },
            target.Context, Repository.GetItem<Lantern>(), Client.Object);

        result!.InteractionMessage.Should().Contain("Taken");
        target.Context.HasItem<Lantern>().Should().BeTrue();
    }

    [Test]
    public async Task Drop_NounMatchesNothingHeld_DoesNotSilentlyDropTheOnlyItemCarried()
    {
        // Issue #502: GetItemsToDrop has the same shape as GetItemsToTake - with a single item in
        // inventory the list-parser returns it for any noun, and the engine dropped it unchecked.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<NorthOfHouse>();
        target.Context.Take(Repository.GetItem<Lantern>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["lantern"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "drop", Noun = "xyzzy", OriginalInput = "drop xyzzy" },
            target.Context, Repository.GetItem<Lantern>(), Client.Object);

        result.Should().BeOfType<NoNounMatchInteractionResult>();
        target.Context.HasItem<Lantern>().Should().BeTrue("the player never named the lantern");
    }

    [Test]
    public async Task Drop_TheItemThePlayerActuallyNamed_IsStillDropped()
    {
        // Issue #502 guard rail for the drop side.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<NorthOfHouse>();
        target.Context.Take(Repository.GetItem<Lantern>());

        var result = await target.GetResponse("drop lantern");

        result.Should().Contain("Dropped");
        target.Context.HasItem<Lantern>().Should().BeFalse();
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