using GameEngine;
using GameEngine.Item;
using GameEngine.Item.ItemProcessor;
using Model.AIGeneration;
using Model.Intent;
using Model.Interaction;
using Model.Interface;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Mech;

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
    public async Task Take_ZeroCandidates_NounLooselyMatchesAPocketedItem_DoesNotSilentlyTakeIt()
    {
        // Issue #536: the zero-candidate fallback (parser returns nothing, as it does in an empty room)
        // resolved action.Noun through Repository.GetItemInScope, whose loose word-boundary containment
        // reaches into worn/held containers. The ID card's bare noun "card" is a trailing word of "lower
        // card", so "take lower card" in a room with nothing takeable quietly pulled the ID card out of a
        // pocket and answered a bare "Taken." - the same silent wrong-item substitution #502 removed from
        // the single-candidate branch, one branch over. The stub returning an EMPTY list is what
        // production really does here; the pre-existing #502 tests all stub a non-empty list.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<NorthOfHouse>();

        var sack = Repository.GetItem<BrownSack>();
        target.Context.Take(sack);
        sack.IsOpen = true;
        var idCard = Repository.GetItem<IdCard>();
        sack.ItemPlacedHere(idCard);

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync([]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "lower card", OriginalInput = "take lower card" },
            target.Context, Repository.GetItem<Lantern>(), Client.Object);

        result.Should().BeOfType<NoNounMatchInteractionResult>();
        target.Context.HasItem<IdCard>().Should().BeFalse("the player never named the ID card");
        idCard.CurrentLocation.Should().Be(sack, "the ID card must stay where it was stowed");
    }

    [Test]
    public async Task Take_ZeroCandidates_RoomItemTheParserCannotDescribe_IsStillTaken()
    {
        // Issue #536 guard rail: the whole reason this fallback exists is the magnet, which is
        // (deliberately, as a puzzle) described as "a metal bar, curved into a U-shape" so the list
        // parser does not recognise it and returns nothing. "take magnet" with the magnet in the room
        // must still succeed - "magnet" is one of the magnet's own nouns, so it precisely names it.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Magnet>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync([]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "magnet", OriginalInput = "take magnet" },
            target.Context, Repository.GetItem<Magnet>(), Client.Object);

        result!.InteractionMessage.Should().Contain("Taken");
        target.Context.HasItem<Magnet>().Should().BeTrue();
    }

    [Test]
    public async Task Take_ZeroCandidates_ItemGenuinelyCarried_IsStillTaken()
    {
        // Issue #536 guard rail: naming something you really are carrying (inside an open held
        // container) must still resolve - the fix rejects a suffix-word coincidence, not a genuine
        // noun that precisely names a carried item.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<NorthOfHouse>();

        var sack = Repository.GetItem<BrownSack>();
        target.Context.Take(sack);
        sack.IsOpen = true;
        sack.ItemPlacedHere(Repository.GetItem<Lunch>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync([]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "lunch", OriginalInput = "take lunch" },
            target.Context, Repository.GetItem<Lantern>(), Client.Object);

        result!.InteractionMessage.Should().Contain("Taken");
        target.Context.HasItem<Lunch>().Should().BeTrue("the player named the lunch and is carrying it");
    }

    [Test]
    public async Task Take_MultiCandidate_SingleObjectNamed_TakesOnlyThatNoun_NotTheWholeRoom()
    {
        // Issue #537: the take list-parser only sees the room description, so when the player names one
        // object that is NOT in the room (the reported case was "take medicine" with the medicine inside
        // a held, open bottle) it returns the room's whole contents. The engine treated that multi-
        // element list as "the player asked for all of these" and swept every portable item in the room
        // - without picking up the thing actually named. A single-object command must follow the noun.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Sword>());
        location.ItemPlacedHere(Repository.GetItem<Lantern>());
        location.ItemPlacedHere(Repository.GetItem<Rope>());

        var sack = Repository.GetItem<BrownSack>();
        target.Context.Take(sack);
        sack.IsOpen = true;
        sack.ItemPlacedHere(Repository.GetItem<Lunch>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["sword", "lantern", "rope"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "lunch", OriginalInput = "take lunch" },
            target.Context, Repository.GetItem<Sword>(), Client.Object);

        result!.InteractionMessage.Should().Contain("Taken");
        target.Context.HasItem<Lunch>().Should().BeTrue("the player asked for the lunch");
        target.Context.HasItem<Sword>().Should().BeFalse("the sword was never named");
        target.Context.HasItem<Lantern>().Should().BeFalse("the lantern was never named");
        target.Context.HasItem<Rope>().Should().BeFalse("the rope was never named");
    }

    [Test]
    public async Task Take_MultiCandidate_QuantifiedRequest_StillTakesEverythingNamed()
    {
        // Issue #537 guard rail: a genuinely quantified command ("take X and Y") must keep its take-all
        // behavior. NamesASingleObject is false here (the input carries " and "), so the multi-take path
        // is left exactly as it was.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Sword>());
        location.ItemPlacedHere(Repository.GetItem<Lantern>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["sword", "lantern"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "sword", OriginalInput = "take the sword and the lantern" },
            target.Context, Repository.GetItem<Sword>(), Client.Object);

        target.Context.HasItem<Sword>().Should().BeTrue();
        target.Context.HasItem<Lantern>().Should().BeTrue();
    }

    [Test]
    public async Task Drop_MultiCandidate_SingleObjectNamed_DropsOnlyThatNoun_NotEverythingHeld()
    {
        // Issue #537 (drop side): GetItemsToDrop has the identical shape - the drop list-parser sees the
        // inventory listing and can return several held items when the player named just one. A single-
        // object command must drop only what the noun resolves to, not the whole inventory.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<NorthOfHouse>();
        target.Context.Take(Repository.GetItem<Sword>());
        target.Context.Take(Repository.GetItem<Lantern>());
        target.Context.Take(Repository.GetItem<Rope>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["sword", "lantern", "rope"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "drop", Noun = "sword", OriginalInput = "drop sword" },
            target.Context, Repository.GetItem<Sword>(), Client.Object);

        result!.InteractionMessage.Should().Contain("Dropped");
        target.Context.HasItem<Sword>().Should().BeFalse("the player asked to drop the sword");
        target.Context.HasItem<Lantern>().Should().BeTrue("the lantern was never named");
        target.Context.HasItem<Rope>().Should().BeTrue("the rope was never named");
    }

    [Test]
    public async Task Drop_MultiCandidate_QuantifiedRequest_StillDropsEverythingNamed()
    {
        // Issue #537 guard rail (drop side): "drop X and Y" must keep dropping both.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<NorthOfHouse>();
        target.Context.Take(Repository.GetItem<Sword>());
        target.Context.Take(Repository.GetItem<Lantern>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["sword", "lantern"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        await processor.Process(
            new SimpleIntent { Verb = "drop", Noun = "sword", OriginalInput = "drop the sword and the lantern" },
            target.Context, Repository.GetItem<Sword>(), Client.Object);

        target.Context.HasItem<Sword>().Should().BeFalse();
        target.Context.HasItem<Lantern>().Should().BeFalse();
    }

    [Test]
    public async Task Take_MultiCandidate_NounLooselyMatchesAPocketedItem_DoesNotSilentlyTakeIt()
    {
        // Issue #536 through the multi-candidate branch: the zero-candidate fix only guards the
        // empty-room path, but the same "take lower card pulls the ID card out of a pocket" substitution
        // is reachable in a POPULATED room. The room-scoped parser returns the room's own items (>1), so
        // the multi-candidate branch runs; following the player's noun through the loose GetItemInScope
        // then drags the pocketed ID card up by its bare noun "card". Resolving the noun must require a
        // *precise* name, exactly as the zero-candidate branch already does.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Sword>());
        location.ItemPlacedHere(Repository.GetItem<Lantern>());

        var sack = Repository.GetItem<BrownSack>();
        target.Context.Take(sack);
        sack.IsOpen = true;
        var idCard = Repository.GetItem<IdCard>();
        sack.ItemPlacedHere(idCard);

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["sword", "lantern"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "lower card", OriginalInput = "take lower card" },
            target.Context, Repository.GetItem<Sword>(), Client.Object);

        result.Should().BeOfType<NoNounMatchInteractionResult>();
        target.Context.HasItem<IdCard>().Should().BeFalse("the player never named the ID card");
        idCard.CurrentLocation.Should().Be(sack, "the ID card must stay where it was stowed");
    }

    [Test]
    public async Task Drop_MultiCandidate_NounLooselyMatchesAHeldItem_DoesNotSilentlyDropIt()
    {
        // Issue #536 through the multi-candidate DROP branch: with several items held the drop parser
        // over-returns, and following the player's noun through the loose GetItemInInventory drags up a
        // top-level-held item whose bare noun ("card" on the ID card) is a trailing word of the typed
        // phrase. DropIt's flat guard only protects nested items, so a top-level ID card would be dropped
        // on "drop lower card" - the same substitution. The resolved item must precisely match the noun.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<NorthOfHouse>();
        var idCard = Repository.GetItem<IdCard>();
        target.Context.Take(idCard);
        target.Context.Take(Repository.GetItem<Sword>());
        target.Context.Take(Repository.GetItem<Lantern>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["sword", "lantern"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "drop", Noun = "lower card", OriginalInput = "drop lower card" },
            target.Context, Repository.GetItem<Sword>(), Client.Object);

        result.Should().BeOfType<NoNounMatchInteractionResult>();
        target.Context.HasItem<IdCard>().Should().BeTrue("the player never named the ID card");
    }

    [Test]
    public async Task Take_SingleCandidate_NounLooselyMatchesAPocketedItem_DoesNotSilentlyTakeIt()
    {
        // Issue #536 through the single-candidate branch - the most common room shape. With exactly one
        // describable item in the room the parser returns just that item, and ItemThePlayerActuallyNamed
        // re-resolves the player's noun when it doesn't match that candidate. That re-resolution used the
        // loose GetItemInScope, so "take lower card" pulled the pocketed ID card up by its bare noun
        // "card" - the same substitution the zero- and multi-candidate branches were already guarded
        // against. The re-resolution must require a precise name too.
        var target = GetTarget();
        var location = Repository.GetLocation<NorthOfHouse>();
        target.Context.CurrentLocation = location;
        location.ItemPlacedHere(Repository.GetItem<Sword>());

        var sack = Repository.GetItem<BrownSack>();
        target.Context.Take(sack);
        sack.IsOpen = true;
        var idCard = Repository.GetItem<IdCard>();
        sack.ItemPlacedHere(idCard);

        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["sword"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "take", Noun = "lower card", OriginalInput = "take lower card" },
            target.Context, Repository.GetItem<Sword>(), Client.Object);

        result.Should().BeOfType<NoNounMatchInteractionResult>();
        target.Context.HasItem<IdCard>().Should().BeFalse("the player never named the ID card");
        target.Context.HasItem<Sword>().Should().BeFalse("the sword was never named either");
        idCard.CurrentLocation.Should().Be(sack, "the ID card must stay where it was stowed");
    }

    [Test]
    public async Task Drop_ZeroCandidates_NounLooselyMatchesATopLevelHeldItem_DoesNotSilentlyDropIt()
    {
        // Issue #536 through the zero-candidate DROP branch: when the drop parser returns nothing the
        // fallback resolved action.Noun through the loose GetItemInInventory, so "drop lower card" with a
        // top-level-held ID card dropped it by its bare noun "card". (DropIt's flat guard only protects
        // nested items.) The zero-candidate drop path must resolve the player's noun strictly, like the
        // take side.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<NorthOfHouse>();
        var idCard = Repository.GetItem<IdCard>();
        target.Context.Take(idCard);

        TakeAndDropParser.Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync([]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "drop", Noun = "lower card", OriginalInput = "drop lower card" },
            target.Context, Repository.GetItem<Sword>(), Client.Object);

        result.Should().BeOfType<NoNounMatchInteractionResult>();
        target.Context.HasItem<IdCard>().Should().BeTrue("the player never named the ID card");
    }

    [Test]
    public async Task Drop_SingleCandidate_NounLooselyMatchesAHeldItem_DoesNotSilentlyDropIt()
    {
        // Issue #536 through the single-candidate DROP branch - the mirror of the take-side regression.
        // With one droppable item the drop parser returns just it, and ItemThePlayerActuallyNamed
        // re-resolves the player's noun when it doesn't match that candidate. That re-resolution routes
        // through the same StrictlyNamedItem seam as take, so "drop lower card" must not drop a top-level
        // ID card by its bare noun "card".
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<NorthOfHouse>();
        var idCard = Repository.GetItem<IdCard>();
        target.Context.Take(idCard);
        target.Context.Take(Repository.GetItem<Sword>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["sword"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "drop", Noun = "lower card", OriginalInput = "drop lower card" },
            target.Context, Repository.GetItem<Sword>(), Client.Object);

        result.Should().BeOfType<NoNounMatchInteractionResult>();
        target.Context.HasItem<IdCard>().Should().BeTrue("the player never named the ID card");
        target.Context.HasItem<Sword>().Should().BeTrue("the sword was never named either");
    }

    [Test]
    public async Task Drop_MultiCandidate_NamedItemIsNested_RefusesWithoutDroppingHeldItems()
    {
        // Issue #537 drop plan point 5: the named item resolves precisely but is nested inside an open
        // held container. DropIt is deliberately flat (only top-level held items drop), so the player must
        // take a nested item out first - the command must produce "You don't have that!" for the nested
        // item WITHOUT dropping the other held items the over-returning parser listed.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<NorthOfHouse>();

        var sack = Repository.GetItem<BrownSack>();
        target.Context.Take(sack);
        sack.IsOpen = true;
        var lunch = Repository.GetItem<Lunch>();
        sack.ItemPlacedHere(lunch);
        target.Context.Take(Repository.GetItem<Sword>());
        target.Context.Take(Repository.GetItem<Lantern>());

        TakeAndDropParser.Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(["sword", "lantern"]);

        IVerbProcessor processor = new TakeOrDropInteractionProcessor(TakeAndDropParser.Object);
        var result = await processor.Process(
            new SimpleIntent { Verb = "drop", Noun = "lunch", OriginalInput = "drop lunch" },
            target.Context, Repository.GetItem<Sword>(), Client.Object);

        result!.InteractionMessage.Should().Contain("don't have that");
        lunch.CurrentLocation.Should().Be(sack, "a nested item can't be dropped until it's taken out");
        target.Context.HasItem<Sword>().Should().BeTrue("the sword was never named");
        target.Context.HasItem<Lantern>().Should().BeTrue("the lantern was never named");
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