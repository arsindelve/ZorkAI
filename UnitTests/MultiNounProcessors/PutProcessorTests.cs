using Game.Item.MultiItemProcessor;
using Model.Intent;
using Model.Interface;
using Model.Item;
using UnitTests.SingleNounProcessors;
using ZorkOne;

namespace UnitTests.MultiNounProcessors;

public class PutProcessorTests : EngineTestsBase
{
    [Test]
    public void PutProcessor_WrongItemOneType()
    {
        var target = new PutProcessor();
        Assert.Throws<Exception>(() => target.Process(null!, null!, null!, new MadeUpItem()));
    }

    [Test]
    public void PutProcessor_ReceiverCannotHoldItems()
    {
        var target = new PutProcessor();
        var result = target.Process(null!, null!, new Lantern()!, new Sword());
        result.Should().BeNull();
    }

    [Test]
    public void PutProcessor_NoVerbMatch()
    {
        var target = new PutProcessor();

        // Act
        var result = target.Process(new MultiNounIntent
        {
            Verb = "cut",
            NounOne = "lantern",
            NounTwo = "mailbox",
            Preposition = "with",
            OriginalInput = ""
        }, null!, new Lantern()!, new Mailbox());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void PutProcessor_NoPrepositionMatch()
    {
        var target = new PutProcessor();

        // Act
        var result = target.Process(new MultiNounIntent
        {
            Verb = "put",
            NounOne = "lantern",
            NounTwo = "mailbox",
            Preposition = "on",
            OriginalInput = ""
        }, null!, new Lantern()!, new Mailbox());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void PutProcessor_IDontHaveTheThing()
    {
        var target = new PutProcessor();

        // Act
        var result = target.Process(new MultiNounIntent
        {
            Verb = "put",
            NounOne = "lantern",
            NounTwo = "mailbox",
            Preposition = "inside",
            OriginalInput = ""
        }, null!, new Lantern()!, new Mailbox());

        // Assert
        result.Should().NotBeNull();
        result!.InteractionHappened.Should().BeTrue();
        result.InteractionMessage.Should().Contain("You don't have the lantern");
    }

    [Test]
    public void PutProcessor_TheContainerIsClosed()
    {
        var target = new PutProcessor();
        var lantern = Repository.GetItem<Lantern>();
        lantern.CurrentLocation = new Context<ZorkI>();

        // Act
        var result = target.Process(new MultiNounIntent
        {
            Verb = "put",
            NounOne = "lantern",
            NounTwo = "mailbox",
            Preposition = "inside",
            OriginalInput = ""
        }, null!, lantern, new Mailbox());

        // Assert
        result.Should().NotBeNull();
        result!.InteractionHappened.Should().BeTrue();
        result.InteractionMessage.Should().Contain("closed");
    }

    [Test]
    public void PutProcessor_MailBoxOneItemSuccess()
    {
        var target = new PutProcessor();
        var leaflet = Repository.GetItem<Leaflet>();
        var mailbox = Repository.GetItem<Mailbox>();
        mailbox.RemoveItem(leaflet);
        ((IOpenAndClose)mailbox).IsOpen = true;
        leaflet.CurrentLocation = new Context<ZorkI>();

        // Act
        var result = target.Process(new MultiNounIntent
        {
            Verb = "put",
            NounOne = "leaflet",
            NounTwo = "mailbox",
            Preposition = "inside",
            OriginalInput = ""
        }, null!, leaflet, mailbox);

        // Assert
        result.Should().NotBeNull();
        result!.InteractionHappened.Should().BeTrue();
        result.InteractionMessage.Should().Contain("Done");
        leaflet.CurrentLocation.Should().Be(mailbox);
    }

    [Test]
    public void PutProcessor_MailBoxRope_Success()
    {
        var target = new PutProcessor();
        var leaflet = Repository.GetItem<Leaflet>();

        var rope = Repository.GetItem<Rope>();
        var mailbox = Repository.GetItem<Mailbox>();
        mailbox.RemoveItem(leaflet);
        ((IOpenAndClose)mailbox).IsOpen = true;
        rope.CurrentLocation = new Context<ZorkI>();

        // Act
        var result = target.Process(new MultiNounIntent
        {
            Verb = "put",
            NounOne = "rope",
            NounTwo = "mailbox",
            Preposition = "inside",
            OriginalInput = ""
        }, null!, rope, mailbox);

        // Assert
        result.Should().NotBeNull();
        result!.InteractionHappened.Should().BeTrue();
        result.InteractionMessage.Should().Contain("Done");
        rope.CurrentLocation.Should().Be(mailbox);
    }

    [Test]
    public void PutProcessor_MailBoxLantern_Fail()
    {
        var target = new PutProcessor();
        var leaflet = Repository.GetItem<Leaflet>();

        var lantern = Repository.GetItem<Lantern>();
        var mailbox = Repository.GetItem<Mailbox>();
        mailbox.RemoveItem(leaflet);
        ((IOpenAndClose)mailbox).IsOpen = true;
        lantern.CurrentLocation = new Context<ZorkI>(Mock.Of<IGameEngine>(), new ZorkI());

        // Act
        var result = target.Process(new MultiNounIntent
        {
            Verb = "put",
            NounOne = "lantern",
            NounTwo = "mailbox",
            Preposition = "inside",
            OriginalInput = ""
        }, null!, lantern, mailbox);

        // Assert
        result.Should().NotBeNull();
        result!.InteractionHappened.Should().BeTrue();
        result.InteractionMessage.Should().Contain("no room");
    }

    [Test]
    public void PutProcessor_CaseCanHoldTwoLargeItems()
    {
        var target = new PutProcessor();
        var sword = Repository.GetItem<Sword>();
        var painting = Repository.GetItem<Painting>();
        var trophyCase = Repository.GetItem<TrophyCase>();
        ((IOpenAndClose)trophyCase).IsOpen = true;
        painting.CurrentLocation = trophyCase;
        sword.CurrentLocation = new Context<ZorkI>();

        // Act
        var result = target.Process(new MultiNounIntent
        {
            Verb = "put",
            NounOne = "sword",
            NounTwo = "case",
            Preposition = "inside",
            OriginalInput = ""
        }, null!, sword, trophyCase);

        // Assert
        result.Should().NotBeNull();
        result!.InteractionHappened.Should().BeTrue();
        result.InteractionMessage.Should().Contain("Done");
        painting.CurrentLocation.Should().Be(trophyCase);
        sword.CurrentLocation.Should().Be(trophyCase);
    }

    [Test]
    public void PutProcessor_MailBoxNoRoomForLargeItem()
    {
        var target = new PutProcessor();
        var sword = Repository.GetItem<Sword>();
        var mailbox = Repository.GetItem<Mailbox>();
        ((IOpenAndClose)mailbox).IsOpen = true;
        sword.CurrentLocation = new Context<ZorkI>();

        // Act
        var result = target.Process(new MultiNounIntent
        {
            Verb = "put",
            NounOne = "sword",
            NounTwo = "mailbox",
            Preposition = "inside",
            OriginalInput = ""
        }, null!, sword, mailbox);

        // Assert
        result.Should().NotBeNull();
        result!.InteractionHappened.Should().BeTrue();
        result.InteractionMessage.Should().Contain("no room");
        sword.CurrentLocation.Should().NotBe(mailbox);
    }

    [Test]
    public void PutProcessor_NothingCanGoInTheBottle()
    {
        var target = new PutProcessor();
        var garlic = Repository.GetItem<Garlic>();
        var bottle = Repository.GetItem<Bottle>();
        ((IOpenAndClose)bottle).IsOpen = true;
        garlic.CurrentLocation = new Context<ZorkI>();

        // Act
        var result = target.Process(new MultiNounIntent
        {
            Verb = "put",
            NounOne = "garlic",
            NounTwo = "bottle",
            Preposition = "inside",
            OriginalInput = ""
        }, null!, garlic, bottle);

        // Assert
        result.Should().NotBeNull();
        result!.InteractionHappened.Should().BeTrue();
        result.InteractionMessage.Should().Contain("no room");
        garlic.CurrentLocation.Should().NotBe(bottle);
    }

    [Test]
    public void PutProcessor_MailboxOnlyHoldTwoSmallItems()
    {
        var target = new PutProcessor();
        var garlic = Repository.GetItem<Garlic>();
        var knife = Repository.GetItem<NastyKnife>();
        var leaflet = Repository.GetItem<Leaflet>();
        var mailbox = Repository.GetItem<Mailbox>();

        mailbox.ItemPlacedHere(leaflet);
        mailbox.ItemPlacedHere(knife);

        ((IOpenAndClose)mailbox).IsOpen = true;
        var context = new Context<ZorkI>();
        garlic.CurrentLocation = context;
        knife.CurrentLocation = context;
        leaflet.CurrentLocation = mailbox;

        // Act
        var result = target.Process(new MultiNounIntent
        {
            Verb = "put",
            NounOne = "garlic",
            NounTwo = "mailbox",
            Preposition = "inside",
            OriginalInput = ""
        }, null!, garlic, mailbox);

        // Assert
        result.Should().NotBeNull();
        result!.InteractionHappened.Should().BeTrue();
        result.InteractionMessage.Should().Contain("no room");
        garlic.CurrentLocation.Should().NotBe(mailbox);
    }
}