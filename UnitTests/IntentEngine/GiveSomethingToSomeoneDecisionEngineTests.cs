using GameEngine;
using GameEngine.IntentEngine;
using Model.Intent;
using Model.Interface;
using Model.Interaction;
using Model.Item;
using Model.Location;

namespace UnitTests.IntentEngine;

[TestFixture]
public class GiveSomethingToSomeoneDecisionEngineTests
{
    [SetUp]
    public void SetUp()
    {
        Repository.Reset();
    }

    [TearDown]
    public void TearDown()
    {
        Repository.Reset();
    }

    [TestFixture]
    public class VerbMatchingTests : GiveSomethingToSomeoneDecisionEngineTests
    {
        [Test]
        public void Should_ReturnNull_When_VerbIsNotGiveVerb()
        {
            // Arrange
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "attack",
                NounOne = "troll",
                NounTwo = "sword",
                Preposition = "with",
                OriginalInput = "attack troll with sword"
            };
            var mockContext = new Mock<IContext>();

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_ProcessIntent_When_VerbIsGive()
        {
            // Arrange
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var lunch = Repository.GetItem<Lunch>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "lunch",
                NounTwo = "troll",
                Preposition = "to",
                OriginalInput = "give lunch to troll"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([lunch]);
            mockContext.Setup(c => c.HasMatchingNoun("lunch", true)).Returns((true, lunch));

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void Should_ProcessIntent_When_VerbIsOffer()
        {
            // Arrange
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var lunch = Repository.GetItem<Lunch>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "offer",
                NounOne = "lunch",
                NounTwo = "troll",
                Preposition = "to",
                OriginalInput = "offer lunch to troll"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([lunch]);
            mockContext.Setup(c => c.HasMatchingNoun("lunch", true)).Returns((true, lunch));

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void Should_ProcessIntent_When_VerbIsTransfer()
        {
            // Arrange
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var lunch = Repository.GetItem<Lunch>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "transfer",
                NounOne = "lunch",
                NounTwo = "troll",
                Preposition = "to",
                OriginalInput = "transfer lunch to troll"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([lunch]);
            mockContext.Setup(c => c.HasMatchingNoun("lunch", true)).Returns((true, lunch));

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void Should_ProcessIntent_When_VerbIsPresent()
        {
            // Arrange
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var lunch = Repository.GetItem<Lunch>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "present",
                NounOne = "lunch",
                NounTwo = "troll",
                Preposition = "to",
                OriginalInput = "present lunch to troll"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([lunch]);
            mockContext.Setup(c => c.HasMatchingNoun("lunch", true)).Returns((true, lunch));

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
        }
    }

    [TestFixture]
    public class NounOrderingTests : GiveSomethingToSomeoneDecisionEngineTests
    {
        [Test]
        public void Should_IdentifyRecipient_When_NounTwoIsRecipient()
        {
            // Arrange: "give axe to troll" pattern
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var axe = Repository.GetItem<BloodyAxe>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "axe",
                NounTwo = "troll",
                Preposition = "to",
                OriginalInput = "give axe to troll"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([axe]);
            mockContext.Setup(c => c.HasMatchingNoun("axe", true)).Returns((true, axe));
            axe.CurrentLocation = mockContext.Object;

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PositiveInteractionResult>();
        }

        [Test]
        public void Should_IdentifyRecipient_When_NounOneIsRecipient()
        {
            // Arrange: "offer troll the axe" pattern
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var axe = Repository.GetItem<BloodyAxe>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "offer",
                NounOne = "troll",
                NounTwo = "axe",
                Preposition = "the",
                OriginalInput = "offer troll the axe"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([axe]);
            mockContext.Setup(c => c.HasMatchingNoun("axe", true)).Returns((true, axe));
            axe.CurrentLocation = mockContext.Object;

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PositiveInteractionResult>();
        }

        [Test]
        public void Should_ReturnNull_When_RecipientNotInEitherNoun()
        {
            // Arrange: Neither noun matches the troll
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "sword",
                NounTwo = "mailbox",
                Preposition = "to",
                OriginalInput = "give sword to mailbox"
            };
            var mockContext = new Mock<IContext>();

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_IdentifyItem_When_MonsterNounUsed()
        {
            // Arrange: Troll can be referred to as "monster"
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var lunch = Repository.GetItem<Lunch>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "lunch",
                NounTwo = "monster",
                Preposition = "to",
                OriginalInput = "give lunch to monster"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([lunch]);
            mockContext.Setup(c => c.HasMatchingNoun("lunch", true)).Returns((true, lunch));

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
        }
    }

    [TestFixture]
    public class ItemValidationTests : GiveSomethingToSomeoneDecisionEngineTests
    {
        [Test]
        public void Should_ReturnNull_When_ItemDoesNotExistInRepository()
        {
            // Arrange: Try to give a non-existent item
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "unicorn",
                NounTwo = "troll",
                Preposition = "to",
                OriginalInput = "give unicorn to troll"
            };
            var mockContext = new Mock<IContext>();

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_ReturnMessage_When_ItemNotInInventory()
        {
            // Arrange: Item exists in room but not in player's inventory
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var lunch = Repository.GetItem<Lunch>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "lunch",
                NounTwo = "troll",
                Preposition = "to",
                OriginalInput = "give lunch to troll"
            };

            // Set up location with lunch in it
            var mockLocation = new Mock<ILocation>();
            mockLocation.As<ICanContainItems>(); // Locations implement ICanContainItems
            mockLocation.Setup(l => l.HasMatchingNoun("lunch", true)).Returns((true, lunch));

            // Set lunch's location so IsItemAccessible can validate the hierarchy
            lunch.CurrentLocation = mockLocation.As<ICanContainItems>().Object;

            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([]); // Empty inventory
            mockContext.Setup(c => c.HasMatchingNoun("lunch", true)).Returns((false, null)); // Not in inventory
            mockContext.Setup(c => c.CurrentLocation).Returns(mockLocation.Object); // Lunch is in the room

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PositiveInteractionResult>();
            result.InteractionMessage.Should().Contain("You don't have the");
            result.InteractionMessage.Should().Contain("lunch");
        }

        [Test]
        public void Should_CallOfferThisThing_When_ItemInInventory()
        {
            // Arrange: Item is in inventory, should delegate to recipient
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var lunch = Repository.GetItem<Lunch>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "lunch",
                NounTwo = "troll",
                Preposition = "to",
                OriginalInput = "give lunch to troll"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([lunch]);
            mockContext.Setup(c => c.HasMatchingNoun("lunch", true)).Returns((true, lunch));

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            // Troll should throw back the lunch (not axe)
            result.InteractionMessage.Should().Contain("The troll");
        }

        [Test]
        public void Should_WorkWithRealTroll_AcceptingAxe()
        {
            // Arrange: Integration test - Troll accepts bloody axe specially
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var axe = Repository.GetItem<BloodyAxe>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "axe",
                NounTwo = "troll",
                Preposition = "to",
                OriginalInput = "give axe to troll"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([axe]);
            mockContext.Setup(c => c.Take(It.IsAny<IItem>()));
            mockContext.Setup(c => c.HasMatchingNoun("axe", true)).Returns((true, axe));
            axe.CurrentLocation = mockContext.Object;

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PositiveInteractionResult>();
            // Troll should accept the axe (holding it, not containing it)
            troll.ItemBeingHeld.Should().Be(axe);
        }

        [Test]
        public void Should_WorkWithRealCyclops_AcceptingLunch()
        {
            // Arrange: Integration test - Cyclops accepts lunch
            Repository.Reset();
            var cyclops = Repository.GetItem<Cyclops>();
            var lunch = Repository.GetItem<Lunch>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Cyclops>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "lunch",
                NounTwo = "cyclops",
                Preposition = "to",
                OriginalInput = "give lunch to cyclops"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([lunch]);
            mockContext.Setup(c => c.HasMatchingNoun("lunch", true)).Returns((true, lunch));

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, cyclops, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PositiveInteractionResult>();
            // Cyclops should react to lunch
            result.InteractionMessage.Should().Contain("cyclops");
        }
    }

    [TestFixture]
    public class RecipientInteractionTests : GiveSomethingToSomeoneDecisionEngineTests
    {
        [Test]
        public void Should_DelegateTroll_AcceptsAxeSpecially()
        {
            // Arrange: Troll has special behavior for bloody axe
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var axe = Repository.GetItem<BloodyAxe>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "axe",
                NounTwo = "troll",
                Preposition = "to",
                OriginalInput = "give bloody axe to troll"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([axe]);
            mockContext.Setup(c => c.HasMatchingNoun("axe", true)).Returns((true, axe));
            mockContext.As<ICanContainItems>().Setup(c => c.RemoveItem(It.IsAny<IItem>()));
            axe.CurrentLocation = mockContext.Object;

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PositiveInteractionResult>();
            // Troll should accept the axe (holding it, not containing it)
            troll.ItemBeingHeld.Should().Be(axe);
        }

        [Test]
        public void Should_DelegateTroll_ThrowsBackOtherItems()
        {
            // Arrange: Troll throws back items that aren't the axe
            Repository.Reset();
            var troll = Repository.GetItem<Troll>();
            var sword = Repository.GetItem<Sword>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Troll>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "sword",
                NounTwo = "troll",
                Preposition = "to",
                OriginalInput = "give sword to troll"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([sword]);
            mockContext.Setup(c => c.HasMatchingNoun("sword", true)).Returns((true, sword));

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, troll, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result.InteractionMessage.Should().Contain("The troll");
            // Troll should not have the sword
            troll.HasItem<Sword>().Should().BeFalse();
        }

        [Test]
        public void Should_DelegateCyclops_AcceptsLunch()
        {
            // Arrange: Cyclops accepts lunch and becomes agitated
            Repository.Reset();
            var cyclops = Repository.GetItem<Cyclops>();
            var lunch = Repository.GetItem<Lunch>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Cyclops>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "lunch",
                NounTwo = "cyclops",
                Preposition = "to",
                OriginalInput = "give lunch to cyclops"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([lunch]);
            mockContext.Setup(c => c.HasMatchingNoun("lunch", true)).Returns((true, lunch));

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, cyclops, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PositiveInteractionResult>();
            result.InteractionMessage.Should().Contain("cyclops");
        }

        [Test]
        public void Should_DelegateCyclops_RejectsGarlic()
        {
            // Arrange: Cyclops rejects garlic
            Repository.Reset();
            var cyclops = Repository.GetItem<Cyclops>();
            var garlic = Repository.GetItem<Garlic>();
            var engine = new GiveSomethingToSomeoneDecisionEngine<Cyclops>();
            var intent = new MultiNounIntent
            {
                Verb = "give",
                NounOne = "garlic",
                NounTwo = "cyclops",
                Preposition = "to",
                OriginalInput = "give garlic to cyclops"
            };
            var mockContext = new Mock<IContext>();
            mockContext.Setup(c => c.Items).Returns([garlic]);
            mockContext.Setup(c => c.HasMatchingNoun("garlic", true)).Returns((true, garlic));

            // Act
            var result = engine.AreWeGivingSomethingToSomeone(intent, cyclops, mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result.InteractionMessage.Should().Contain("cyclops");
            result.InteractionMessage.Should().Contain("limit");
        }
    }
}
