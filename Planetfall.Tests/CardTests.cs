using FluentAssertions;
using GameEngine;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

public class CardTests : EngineTestsBase
{
    [Test]
    public async Task Kitchen_HappyPath()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());

        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("slide kitchen access card through slot");

        response.Should().Contain("The kitchen door quietly slides open");
    }

    [Test]
    public async Task Kitchen_Disambiguation_HappyPath_StepOne()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());

        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("slide card through slot");

        response.Should().Contain("Do you mean the kitchen access card or the shuttle access card?");
    }

    [TestCase("kitchen")]
    [TestCase("kitchen card")]
    [TestCase("kitchen access card")]
    [TestCase("kitchen access")]
    [Test]
    public async Task Kitchen_Disambiguation_HappyPath_StepTwo(string input)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());

        engine.Context.CurrentLocation = room;

        await engine.GetResponse("slide card through slot");
        var response = await engine.GetResponse(input);

        response.Should().Contain("The kitchen door quietly slides open");
    }

    [TestCase("kitchen")]
    [TestCase("kitchen card")]
    [TestCase("kitchen access card")]
    [TestCase("kitchen access")]
    [Test]
    public async Task Kitchen_Disambiguation_HappyPath_AnotherExample(string input)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());

        engine.Context.CurrentLocation = room;

        await engine.GetResponse("slide access card through slot");
        var response = await engine.GetResponse(input);

        response.Should().Contain("The kitchen door quietly slides open");
    }

    [Test]
    public async Task Kitchen_WrongCard()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());

        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("slide shuttle access card through slot");

        response.Should().Contain("A sign flashes \"Inkorekt awtharazaashun kard...akses deeniid.\"");
    }

    [Test]
    [TestCase("kitchen")]
    [TestCase("card")]
    [TestCase("kitchen access")]
    [TestCase("kitchen card")]
    [TestCase("kitchen access card")]
    public async Task Single_Card(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());

        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse($"drop {reply}");

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(0);
        Repository.GetItem<KitchenAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }

    [Test]
    [TestCase("kitchen")]
    [TestCase("kitchen access")]
    [TestCase("kitchen card")]
    [TestCase("kitchen access card")]
    public async Task Double_Card(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<UpperElevatorAccessCard>());

        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse($"drop {reply}");

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(1);
        Repository.GetItem<KitchenAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }

    [Test]
    [TestCase("kitchen")]
    [TestCase("kitchen access")]
    [TestCase("kitchen card")]
    [TestCase("kitchen access card")]
    public async Task Disambiguation_Card_Drop_Kitchen(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<UpperElevatorAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());

        engine.Context.CurrentLocation = room;

        await engine.GetResponse("drop card");
        var response = await engine.GetResponse(reply);

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(2);
        Repository.GetItem<KitchenAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }

    [Test]
    [TestCase("shuttle")]
    [TestCase("shuttle access")]
    [TestCase("shuttle card")]
    [TestCase("shuttle access card")]
    public async Task Disambiguation_Card_Drop_Shuttle(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<UpperElevatorAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());

        engine.Context.CurrentLocation = room;

        Console.WriteLine(await engine.GetResponse("drop card"));
        var response = await engine.GetResponse(reply);

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(2);
        Repository.GetItem<ShuttleAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }

    [Test]
    [TestCase("shuttle")]
    [TestCase("shuttle access")]
    [TestCase("shuttle card")]
    [TestCase("shuttle access card")]
    public async Task Disambiguation_CardAccess_Drop_Shuttle(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<UpperElevatorAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<LowerElevatorAccessCard>());

        engine.Context.CurrentLocation = room;

        Console.WriteLine(await engine.GetResponse("drop the access card"));
        var response = await engine.GetResponse(reply);

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(3);
        Repository.GetItem<ShuttleAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }

    [Test]
    [TestCase("upper")]
    [TestCase("upper access")]
    [TestCase("upper card")]
    [TestCase("upper access card")]
    [TestCase("upper elevator access card")]
    [TestCase("upper elevator card")]
    public async Task Disambiguation_Card_Drop_UpperElevator(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<UpperElevatorAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<LowerElevatorAccessCard>());

        engine.Context.CurrentLocation = room;

        Console.WriteLine(await engine.GetResponse("drop card"));
        var response = await engine.GetResponse(reply);

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(3);
        Repository.GetItem<UpperElevatorAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }

    [Test]
    public async Task UseCardOnSlot_Kitchen_OpensKitchenDoor()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("use kitchen access card on slot");

        response.Should().Contain("The kitchen door quietly slides open");
    }

    [Test]
    public async Task UseCardInSlot_Kitchen_OpensKitchenDoor()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("use kitchen access card in slot");

        response.Should().Contain("The kitchen door quietly slides open");
    }

    [Test]
    public async Task UseCardOnSlot_Kitchen_ShortNoun()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("use kitchen card on slot");

        response.Should().Contain("The kitchen door quietly slides open");
    }

    [Test]
    public async Task UseCardOnSlot_WrongCard_Rejected()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("use shuttle access card on slot");

        response.Should().Contain("Inkorekt awtharazaashun kard");
    }

    // Issue #211 - a card scrambled by the magnet (WRONG-CARD, globals.zil:1438) is rejected by its
    // own slot even though it's the correct card type.
    [Test]
    public async Task SlideScrambledCard_ThroughItsOwnSlot_IsRejected()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        var card = Repository.GetItem<KitchenAccessCard>();
        card.Scrambled = true;
        engine.Context.ItemPlacedHere(card);
        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("slide kitchen access card through slot");

        response.Should().Contain("Inkorekt awtharazaashun kard...akses deeniid.");
        Repository.GetItem<KitchenDoor>().IsOpen.Should().BeFalse();
    }

    [Test]
    public async Task SlideUnscrambledCard_ThroughItsOwnSlot_StillOpensDoor()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        var card = Repository.GetItem<KitchenAccessCard>();
        engine.Context.ItemPlacedHere(card);
        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("slide kitchen access card through slot");

        response.Should().Contain("The kitchen door quietly slides open");
        card.Scrambled.Should().BeFalse();
    }

    [Test]
    public async Task SlideCardThroughSlot_StillWorks()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("slide kitchen access card through slot");

        response.Should().Contain("The kitchen door quietly slides open");
    }

    [Test]
    [TestCase(true)] // shuttle carried first - the issue's reported failing order
    [TestCase(false)] // kitchen carried first
    public void KitchenCard_ResolvesToKitchenNotShuttle_RegardlessOfInventoryOrder(bool shuttleFirst)
    {
        // Issue #246, resolver-level guard for the card-collision family. The shuttle card's bare
        // noun "card" is contained in "kitchen card", so an adjective-blind matcher returns the
        // shuttle card when it is first in inventory. This locks in that the SHARED adjective-aware
        // resolver - GetPreciseMatchInScope (the pass MultiNounEngine.IsItemHere now runs first) and
        // GetItemInScope (the single-noun #244 path) - lands on the KITCHEN card regardless of order,
        // guarding the cards' NounsForPreciseMatching definitions against future drift.
        //
        // This asserts the resolver directly, not the engine turn: the end-to-end multi-noun engine
        // path (IsItemHere) is proven red-before/green-after by the bedistor test
        // CourseControlTests.PutGoodBedistorInCube_WhenFusedIsFirstInInventory... A card slide/put
        // cannot stand in for it, because SlotBase resolves cards by type (Match<KitchenAccessCard,
        // KitchenSlot> + HasItem<KitchenAccessCard>) and returns before IsItemHere is ever reached.
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<MessHall>();

        if (shuttleFirst)
        {
            engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
            engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        }
        else
        {
            engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
            engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
        }

        var kitchen = Repository.GetItem<KitchenAccessCard>();

        Repository.GetPreciseMatchInScope("kitchen card", engine.Context).Should().Be(kitchen);
        Repository.GetItemInScope("kitchen card", engine.Context).Should().Be(kitchen);
    }
}
