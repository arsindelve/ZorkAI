using GameEngine;
using GameEngine.IntentEngine;
using Model.Intent;
using Model.Interaction;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace UnitTests.IntentEngine;

[TestFixture]
public class KillSomeoneDecisionEngineTests
{
    [SetUp]
    public void SetUp()
    {
        Repository.Reset();
        _mockCombatEngine = new Mock<ICombatEngine>();
        _mockContext = new Mock<IContext>();
    }

    [TearDown]
    public void TearDown()
    {
        Repository.Reset();
    }

    private Mock<ICombatEngine> _mockCombatEngine = null!;
    private Mock<IContext> _mockContext = null!;

    [TestFixture]
    public class DoYouWantToKillSomeoneTests : KillSomeoneDecisionEngineTests
    {
        [Test]
        public void Should_ReturnNull_When_NounDoesNotMatchFoe()
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new MultiNounIntent
            {
                Verb = "kill",
                NounOne = "cyclops", // Wrong foe
                NounTwo = "sword",
                Preposition = "with",
                OriginalInput = "kill cyclops with sword"
            };

            // Act
            var result = engine.DoYouWantToKillSomeone(intent, _mockContext.Object);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_ReturnNull_When_VerbIsNotKillVerb()
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new MultiNounIntent
            {
                Verb = "give", // Wrong verb
                NounOne = "troll",
                NounTwo = "sword",
                Preposition = "with",
                OriginalInput = "give troll sword"
            };

            // Act
            var result = engine.DoYouWantToKillSomeone(intent, _mockContext.Object);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_ReturnNull_When_NounTwoIsNotWeapon()
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var lunch = Repository.GetItem<Lunch>();
            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new MultiNounIntent
            {
                Verb = "kill",
                NounOne = "troll",
                NounTwo = "lunch", // Not a weapon
                Preposition = "with",
                OriginalInput = "kill troll with lunch"
            };

            _mockContext.Setup(c => c.CurrentLocation).Returns(Mock.Of<ILocation>());

            // Act
            var result = engine.DoYouWantToKillSomeone(intent, _mockContext.Object);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_ReturnNull_When_PrepositionIsInvalid()
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var sword = Repository.GetItem<Sword>();
            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new MultiNounIntent
            {
                Verb = "kill",
                NounOne = "troll",
                NounTwo = "sword",
                Preposition = "into", // Invalid preposition
                OriginalInput = "kill troll into sword"
            };

            sword.CurrentLocation = _mockContext.Object;
            _mockContext.Setup(c => c.Items).Returns(new List<IItem> { sword });

            // Act
            var result = engine.DoYouWantToKillSomeone(intent, _mockContext.Object);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_ReturnDontHaveWeapon_When_WeaponNotInInventory()
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var sword = Repository.GetItem<Sword>();
            var location = Repository.GetLocation<TrollRoom>();
            location.ItemPlacedHere(sword); // Weapon on ground, not in inventory

            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new MultiNounIntent
            {
                Verb = "kill",
                NounOne = "troll",
                NounTwo = "sword",
                Preposition = "with",
                OriginalInput = "kill troll with sword"
            };

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>()); // Empty inventory

            // Act
            var result = engine.DoYouWantToKillSomeone(intent, _mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result!.InteractionMessage.Should().Contain("don't have");
        }

        [Test]
        [TestCase("kill")]
        [TestCase("attack")]
        [TestCase("defeat")]
        [TestCase("destroy")]
        [TestCase("stab")]
        public void Should_CallCombatEngine_When_ValidKillIntent(string verb)
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var sword = Repository.GetItem<Sword>();
            var location = Repository.GetLocation<TrollRoom>();

            // Put sword in location so it's in scope
            location.ItemPlacedHere(sword);

            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new MultiNounIntent
            {
                Verb = verb,
                NounOne = "troll",
                NounTwo = "sword",
                Preposition = "with",
                OriginalInput = $"{verb} troll with sword"
            };

            // Sword in inventory (CurrentLocation = context)
            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem> { sword });
            _mockContext.Setup(c => c.HasMatchingNoun("sword", true)).Returns((true, sword));
            sword.CurrentLocation = _mockContext.Object;

            _mockCombatEngine.Setup(e => e.Attack(_mockContext.Object, sword))
                .Returns(new PositiveInteractionResult("You attack the troll!"));

            // Act
            var result = engine.DoYouWantToKillSomeone(intent, _mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            _mockCombatEngine.Verify(e => e.Attack(_mockContext.Object, sword), Times.Once);
        }

        [Test]
        [TestCase("with")]
        [TestCase("using")]
        [TestCase("by")]
        [TestCase("to")]
        public void Should_AcceptValidPrepositions(string preposition)
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var sword = Repository.GetItem<Sword>();
            var location = Repository.GetLocation<TrollRoom>();
            location.ItemPlacedHere(sword);

            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new MultiNounIntent
            {
                Verb = "kill",
                NounOne = "troll",
                NounTwo = "sword",
                Preposition = preposition,
                OriginalInput = $"kill troll {preposition} sword"
            };

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem> { sword });
            _mockContext.Setup(c => c.HasMatchingNoun("sword", true)).Returns((true, sword));
            sword.CurrentLocation = _mockContext.Object;

            _mockCombatEngine.Setup(e => e.Attack(_mockContext.Object, sword))
                .Returns(new PositiveInteractionResult("Attack!"));

            // Act
            var result = engine.DoYouWantToKillSomeone(intent, _mockContext.Object);

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void Should_MatchFoeInNounTwo()
        {
            // Arrange - "kill sword at troll" (foe in noun two)
            var troll = Repository.GetItem<Troll>();
            var sword = Repository.GetItem<Sword>();
            sword.CurrentLocation = _mockContext.Object;

            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new MultiNounIntent
            {
                Verb = "attack",
                NounOne = "sword", // Weird but valid
                NounTwo = "troll", // Foe here
                Preposition = "with",
                OriginalInput = "attack sword with troll"
            };

            // This should actually match because NounTwo matches foe
            // But NounTwo should be weapon, not foe - let's verify behavior

            // Act
            var result = engine.DoYouWantToKillSomeone(intent, _mockContext.Object);

            // Assert - Troll is not a weapon, so should return null
            result.Should().BeNull();
        }
    }

    [TestFixture]
    public class DoYouWantToKillSomeoneButYouDidNotSpecifyAWeaponTests : KillSomeoneDecisionEngineTests
    {
        [Test]
        public void Should_ReturnNull_When_VerbIsNotKillVerb()
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new SimpleIntent
            {
                Verb = "give",
                Noun = "troll",
                OriginalInput = "give troll"
            };

            // Act
            var result = engine.DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(intent, _mockContext.Object);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_AttackBareHanded_When_NoWeapons()
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new SimpleIntent
            {
                Verb = "kill",
                Noun = "troll",
                OriginalInput = "kill troll"
            };

            _mockContext.Setup(c => c.GetItems<IWeapon>()).Returns(new List<IWeapon>());
            _mockCombatEngine.Setup(e => e.Attack(_mockContext.Object, null))
                .Returns(new PositiveInteractionResult("You punch the troll!"));

            // Act
            var result = engine.DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(intent, _mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            _mockCombatEngine.Verify(e => e.Attack(_mockContext.Object, null), Times.Once);
        }

        [Test]
        public void Should_UseOnlyWeapon_When_OneWeaponInInventory()
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var sword = Repository.GetItem<Sword>();
            var location = Repository.GetLocation<TrollRoom>();
            location.ItemPlacedHere(sword);
            sword.CurrentLocation = _mockContext.Object;

            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new SimpleIntent
            {
                Verb = "kill",
                Noun = "troll",
                OriginalInput = "kill troll"
            };

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.GetItems<IWeapon>()).Returns(new List<IWeapon> { sword });
            _mockContext.Setup(c => c.Items).Returns(new List<IItem> { sword });
            _mockContext.Setup(c => c.HasMatchingNoun("sword", true)).Returns((true, sword));
            _mockCombatEngine.Setup(e => e.Attack(_mockContext.Object, sword))
                .Returns(new PositiveInteractionResult("You attack!"));

            // Act
            var result = engine.DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(intent, _mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result!.InteractionMessage.Should().Contain("sword");
        }

        [Test]
        public void Should_AskForWeaponChoice_When_MultipleWeapons()
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var sword = Repository.GetItem<Sword>();
            var knife = Repository.GetItem<RustyKnife>();

            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new SimpleIntent
            {
                Verb = "kill",
                Noun = "troll",
                OriginalInput = "kill troll"
            };

            _mockContext.Setup(c => c.GetItems<IWeapon>()).Returns(new List<IWeapon> { sword, knife });

            // Act
            var result = engine.DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(intent, _mockContext.Object);

            // Assert
            result.Should().NotBeNull();
            result!.InteractionMessage.Should().Contain("specify which weapon");
        }

        [Test]
        public void Should_ReturnNull_When_NounDoesNotMatchFoe_AndNoWeapons()
        {
            // Arrange
            var troll = Repository.GetItem<Troll>();
            var engine = new KillSomeoneDecisionEngine<Troll>(_mockCombatEngine.Object);
            var intent = new SimpleIntent
            {
                Verb = "kill",
                Noun = "cyclops", // Wrong foe
                OriginalInput = "kill cyclops"
            };

            _mockContext.Setup(c => c.GetItems<IWeapon>()).Returns(new List<IWeapon>());

            // Act
            var result = engine.DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(intent, _mockContext.Object);

            // Assert
            result.Should().BeNull();
        }
    }
}