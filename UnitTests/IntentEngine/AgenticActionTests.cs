using GameEngine;
using Model.AIGeneration.Requests;
using Model.AIParsing;
using Model.Intent;
using Model.Interface;

namespace UnitTests.IntentEngine;

/// <summary>
/// Issue #136: the agentic fall-through narrator ("narrator with hands"). These deterministic tests
/// prove the ENGINE honors whatever the mocked <see cref="IAgenticActionParser" /> seam returns, fires
/// the seam only behind the inventory gate, and never consults it for commands the game already
/// models. The narrator's own judgment (grounding, plausibility) is proven separately by the
/// [Explicit] prompt-correctness tests in IntegrationTests.
/// </summary>
[TestFixture]
public class AgenticActionTests : EngineTestsBase
{
    /// <summary>
    /// Hook A (single-noun creative verb): a DROP tool call moves the item out of inventory and
    /// onto the ground of the current room, and the seam's narration is what the player sees.
    /// </summary>
    [Test]
    public async Task HookA_DropTool_RemovesFromInventory_LandsInCurrentRoom()
    {
        var target = GetTarget(Mock.Of<IIntentParser>());
        var leaflet = Repository.GetItem<Leaflet>();
        target.Context.Take(leaflet);

        Mock.Get(Parser)
            .Setup(s => s.DetermineComplexIntentType("throw the leaflet in the air", It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new SimpleIntent
                { Verb = "throw", Noun = "leaflet", OriginalInput = "throw the leaflet in the air" });

        AgenticActionParser
            .Setup(s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AgenticActionResult("It flutters back down to the ground.",
                [new AgenticToolCall(AgenticTool.Drop, "leaflet")]));

        var response = await target.GetResponse("throw the leaflet in the air");

        response.Should().Contain("It flutters back down to the ground.");
        target.Context.Items.Should().NotContain(leaflet);
        Repository.GetLocation<WestOfHouse>().Items.Should().Contain(leaflet);
    }

    /// <summary>
    /// Hook A: a DESTROY tool call removes the item from the game entirely (no holder, no location).
    /// </summary>
    [Test]
    public async Task HookA_DestroyTool_RemovesItemFromTheWorld()
    {
        var target = GetTarget(Mock.Of<IIntentParser>());
        var leaflet = Repository.GetItem<Leaflet>();
        target.Context.Take(leaflet);

        Mock.Get(Parser)
            .Setup(s => s.DetermineComplexIntentType("tear up the leaflet", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SimpleIntent { Verb = "tear", Noun = "leaflet", OriginalInput = "tear up the leaflet" });

        AgenticActionParser
            .Setup(s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AgenticActionResult("You shred it to confetti.",
                [new AgenticToolCall(AgenticTool.Destroy, "leaflet")]));

        var response = await target.GetResponse("tear up the leaflet");

        response.Should().Contain("You shred it to confetti.");
        target.Context.Items.Should().NotContain(leaflet);
        Repository.GetItem<Leaflet>().CurrentLocation.Should().BeNull();
    }

    /// <summary>
    /// Hook B (object + destination), the canonical example: "throw the sword into the chasm". The
    /// destination is not a real item anywhere, so today this dies in MultiNounEngine's
    /// missing-second-noun branch ("there's no chasm here") and the sword is untouched. With the
    /// seam, the narrator's DESTROY tool call must actually destroy the sword.
    /// </summary>
    [Test]
    public async Task HookB_MissingDestination_DestroyTool_DestroysHeldItem()
    {
        var target = GetTarget(Mock.Of<IIntentParser>());
        var sword = Repository.GetItem<Sword>();
        target.Context.Take(sword);

        Mock.Get(Parser)
            .Setup(s => s.DetermineComplexIntentType("throw the sword into the chasm", It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new MultiNounIntent
            {
                Verb = "throw", NounOne = "sword", NounTwo = "chasm", Preposition = "into",
                OriginalInput = "throw the sword into the chasm"
            });

        AgenticActionParser
            .Setup(s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AgenticActionResult("It tumbles end over end into the dark and is gone.",
                [new AgenticToolCall(AgenticTool.Destroy, "sword")]));

        var response = await target.GetResponse("throw the sword into the chasm");

        response.Should().Contain("It tumbles end over end into the dark and is gone.");
        target.Context.Items.Should().NotContain(sword);
        Repository.GetItem<Sword>().CurrentLocation.Should().BeNull();
    }

    /// <summary>
    /// Hook B when the destination is scenery that exists only in the room description ("house" at
    /// West of House): the missing-noun branches are skipped but the item-two-is-not-real branch
    /// fires. The seam must be consulted there too.
    /// </summary>
    [Test]
    public async Task HookB_SceneryDestination_DestroyTool_DestroysHeldItem()
    {
        var target = GetTarget(Mock.Of<IIntentParser>());
        var sword = Repository.GetItem<Sword>();
        target.Context.Take(sword);

        Mock.Get(Parser)
            .Setup(s => s.DetermineComplexIntentType("throw the sword at the house", It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new MultiNounIntent
            {
                Verb = "throw", NounOne = "sword", NounTwo = "house", Preposition = "at",
                OriginalInput = "throw the sword at the house"
            });

        AgenticActionParser
            .Setup(s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AgenticActionResult("It clatters off the boarded door and shatters.",
                [new AgenticToolCall(AgenticTool.Destroy, "sword")]));

        var response = await target.GetResponse("throw the sword at the house");

        response.Should().Contain("It clatters off the boarded door and shatters.");
        Repository.GetItem<Sword>().CurrentLocation.Should().BeNull();
    }

    /// <summary>
    /// Hook B when both nouns are real items but no processor models the action ("throw sword at
    /// mailbox"): the generic verb-no-effect fall-through must also consult the seam first.
    /// </summary>
    [Test]
    public async Task HookB_BothNounsReal_NothingHandledIt_DropToolApplies()
    {
        var target = GetTarget(Mock.Of<IIntentParser>());
        var sword = Repository.GetItem<Sword>();
        target.Context.Take(sword);

        Mock.Get(Parser)
            .Setup(s => s.DetermineComplexIntentType("throw the sword at the mailbox", It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new MultiNounIntent
            {
                Verb = "throw", NounOne = "sword", NounTwo = "mailbox", Preposition = "at",
                OriginalInput = "throw the sword at the mailbox"
            });

        AgenticActionParser
            .Setup(s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AgenticActionResult("It bounces off the little door and lands at your feet.",
                [new AgenticToolCall(AgenticTool.Drop, "sword")]));

        var response = await target.GetResponse("throw the sword at the mailbox");

        response.Should().Contain("It bounces off the little door and lands at your feet.");
        target.Context.Items.Should().NotContain(sword);
        Repository.GetLocation<WestOfHouse>().Items.Should().Contain(sword);
    }

    /// <summary>
    /// An empty tool list is a strict no-op: the narrator was uncertain (no river here, can't tear
    /// steel), so the snark is shown and absolutely nothing changes state.
    /// </summary>
    [Test]
    public async Task EmptyToolList_ShowsSnark_ChangesNothing()
    {
        var target = GetTarget(Mock.Of<IIntentParser>());
        var leaflet = Repository.GetItem<Leaflet>();
        target.Context.Take(leaflet);

        Mock.Get(Parser)
            .Setup(s => s.DetermineComplexIntentType("throw the leaflet in the river", It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new SimpleIntent
                { Verb = "throw", Noun = "leaflet", OriginalInput = "throw the leaflet in the river" });

        AgenticActionParser
            .Setup(s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AgenticActionResult("What river? You flail at empty air.", []));

        var response = await target.GetResponse("throw the leaflet in the river");

        response.Should().Contain("What river? You flail at empty air.");
        target.Context.Items.Should().Contain(leaflet);
        Repository.GetItem<Leaflet>().CurrentLocation.Should().Be(target.Context);
    }

    /// <summary>
    /// The trigger gate: the seam is never consulted when the acted-upon noun is not in inventory.
    /// The sword is on the ground here, so the existing missing-second-noun narration runs instead.
    /// </summary>
    [Test]
    public async Task Gate_NounNotInInventory_SeamNeverConsulted()
    {
        var target = GetTarget(Mock.Of<IIntentParser>());
        var sword = Repository.GetItem<Sword>();
        Repository.GetLocation<WestOfHouse>().ItemPlacedHere(sword);

        Mock.Get(Parser)
            .Setup(s => s.DetermineComplexIntentType("throw the sword in the river", It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new MultiNounIntent
            {
                Verb = "throw", NounOne = "sword", NounTwo = "river", Preposition = "in",
                OriginalInput = "throw the sword in the river"
            });

        Client.Setup(s => s.GenerateNarration(It.IsAny<MissingSecondNounMultiNounOperationRequest>(),
                It.IsAny<string>()))
            .ReturnsAsync("There is no river here.");

        var response = await target.GetResponse("throw the sword in the river");

        response.Should().Contain("There is no river here.");
        AgenticActionParser.Verify(
            s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Repository.GetLocation<WestOfHouse>().Items.Should().Contain(sword);
    }

    /// <summary>
    /// Commands the game already models are intercepted upstream and must never reach the seam:
    /// "drop the leaflet" is the real drop mechanic, not the narrator's business.
    /// </summary>
    [Test]
    public async Task HandledCommand_RealDropMechanic_SeamNeverConsulted()
    {
        var target = GetTarget(Mock.Of<IIntentParser>());
        var leaflet = Repository.GetItem<Leaflet>();
        target.Context.Take(leaflet);

        Mock.Get(Parser)
            .Setup(s => s.DetermineComplexIntentType("drop the leaflet", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SimpleIntent { Verb = "drop", Noun = "leaflet", OriginalInput = "drop the leaflet" });

        var response = await target.GetResponse("drop the leaflet");

        response.Should().Contain("Dropped");
        target.Context.Items.Should().NotContain(leaflet);
        AgenticActionParser.Verify(
            s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// The seam must receive the live grounding inputs - the player's raw input, the current
    /// inventory listing, and the current room description - so the prompt CAN refuse ungrounded
    /// destinations. Capture the arguments and check them.
    /// </summary>
    [Test]
    public async Task Seam_ReceivesInventoryAndRoomDescription_ForGrounding()
    {
        var target = GetTarget(Mock.Of<IIntentParser>());
        var leaflet = Repository.GetItem<Leaflet>();
        target.Context.Take(leaflet);

        Mock.Get(Parser)
            .Setup(s => s.DetermineComplexIntentType("tear up the leaflet", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SimpleIntent { Verb = "tear", Noun = "leaflet", OriginalInput = "tear up the leaflet" });

        string? seenInput = null;
        string? seenInventory = null;
        string? seenLocation = null;
        AgenticActionParser
            .Setup(s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback((string playerInput, string inventoryDescription, string locationDescription) =>
            {
                seenInput = playerInput;
                seenInventory = inventoryDescription;
                seenLocation = locationDescription;
            })
            .ReturnsAsync(new AgenticActionResult("Confetti.", [new AgenticToolCall(AgenticTool.Destroy, "leaflet")]));

        await target.GetResponse("tear up the leaflet");

        seenInput.Should().Contain("tear up the leaflet");
        seenInventory.Should().ContainEquivalentOf("leaflet");
        seenLocation.Should().ContainEquivalentOf("white house");
    }
}
