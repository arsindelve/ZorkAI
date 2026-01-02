using FluentAssertions;
using GameEngine;
using Model.Interface;
using Moq;
using ZorkOne.ActorInteraction;
using ZorkOne.Item;

namespace ZorkOne.Tests.People;

public class TrollCombatTests : EngineTestsBase
{
    [Test]
    public void SmallWound()
    {
        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(s =>
                s.Choose(
                    It.IsAny<List<(CombatOutcome outcome, string text)>>()))
            .Returns((CombatOutcome.SmallWound, "The flat of the troll's axe skins across your forearm. "));

        var engine = GetTarget();
        var target = new TrollCombatEngine(chooser.Object);

        // Act
        var result = target.Attack(engine.Context);

        // Assert
        result.Should().Contain("The flat of the troll's");
        engine.Context.LightWoundCounter.Should().Be(30);
    }

    [Test]
    public void Stun()
    {
        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(s =>
                s.Choose(
                    It.IsAny<List<(CombatOutcome outcome, string text)>>()))
            .Returns((CombatOutcome.Stun, "bob"));

        var engine = GetTarget();
        var target = new TrollCombatEngine(chooser.Object);

        // Act
        var result = target.Attack(engine.Context);

        // Assert
        result.Should().Contain("bob");
        engine.Context.IsStunned.Should().BeTrue();
        engine.Context.LightWoundCounter.Should().Be(0);
    }

    [Test]
    public void Miss()
    {
        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(s =>
                s.Choose(
                    It.IsAny<List<(CombatOutcome outcome, string text)>>()))
            .Returns((CombatOutcome.Miss, "miss"));

        var engine = GetTarget();
        var target = new TrollCombatEngine(chooser.Object);

        // Act
        var result = target.Attack(engine.Context);

        // Assert
        result.Should().Contain("miss");
        engine.Context.IsStunned.Should().BeFalse();
        engine.Context.LightWoundCounter.Should().Be(0);
    }

    [Test]
    public void Fatal()
    {
        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(s =>
                s.Choose(
                    It.IsAny<List<(CombatOutcome outcome, string text)>>()))
            .Returns((CombatOutcome.Fatal, "fatal"));

        var engine = GetTarget();
        var target = new TrollCombatEngine(chooser.Object);

        // Act
        var result = target.Attack(engine.Context);

        // Assert
        result.Should().Contain("fatal");
        result.Should().Contain("that that last blow was too much for you");
        result.Should().Contain("This is a forest, with trees in all directions");
        engine.Context.IsStunned.Should().BeFalse();
        engine.Context.LightWoundCounter.Should().Be(0);
        engine.Context.DeathCounter.Should().Be(1);
    }

    [Test]
    public void SecondSmallWound()
    {
        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(s =>
                s.Choose(
                    It.IsAny<List<(CombatOutcome outcome, string text)>>()))
            .Returns((CombatOutcome.SmallWound, "wound"));

        var engine = GetTarget();
        var target = new TrollCombatEngine(chooser.Object);

        // Act
        target.Attack(engine.Context);
        var result = target.Attack(engine.Context);

        // Assert
        result.Should().Contain("wound");
        result.Should().Contain("that that last blow was too much for you");
        result.Should().Contain("This is a forest, with trees in all directions");
        engine.Context.IsStunned.Should().BeFalse();
        engine.Context.LightWoundCounter.Should().Be(0);
        engine.Context.DeathCounter.Should().Be(1);
    }

    [Test]
    public void DropSword()
    {
        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(s =>
                s.Choose(
                    It.IsAny<List<(CombatOutcome outcome, string text)>>()))
            .Returns((CombatOutcome.Disarm, "{weapon}"));

        var engine = GetTarget();
        engine.Context.Take(Repository.GetItem<Sword>());
        var target = new TrollCombatEngine(chooser.Object);

        // Act
        var result = target.Attack(engine.Context);

        // Assert
        result.Should().Contain("sword");
        engine.Context.IsStunned.Should().BeFalse();
        engine.Context.LightWoundCounter.Should().Be(0);
        engine.Context.HasWeapon.Should().BeFalse();
        engine.Context.HasItem<Sword>().Should().BeFalse();
    }

    [Test]
    public void DropKnife()
    {
        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(s =>
                s.Choose(
                    It.IsAny<List<(CombatOutcome outcome, string text)>>()))
            .Returns((CombatOutcome.Disarm, "{weapon}"));

        var engine = GetTarget();
        engine.Context.Take(Repository.GetItem<NastyKnife>());
        var target = new TrollCombatEngine(chooser.Object);

        // Act
        var result = target.Attack(engine.Context);

        // Assert
        result.Should().Contain("knife");
        engine.Context.IsStunned.Should().BeFalse();
        engine.Context.LightWoundCounter.Should().Be(0);
        engine.Context.HasWeapon.Should().BeFalse();
        engine.Context.HasItem<NastyKnife>().Should().BeFalse();
    }

    [TestFixture]
    public class TakeAxeFromTrollTests : TrollCombatTests
    {
        [Test]
        public async Task TakeAxe_TrollAliveAndConscious_ShouldRefuse()
        {
            // Arrange: In troll room, troll holding axe, player has light
            var engine = GetTarget();
            engine.Context.CurrentLocation = Repository.GetLocation<ZorkOne.Location.TrollRoom>();

            var lantern = Repository.GetItem<Lantern>();
            lantern.IsOn = true;
            engine.Context.Take(lantern);

            var troll = Repository.GetItem<Troll>();
            var axe = Repository.GetItem<BloodyAxe>();

            // Verify setup
            troll.ItemBeingHeld.Should().Be(axe);
            troll.IsUnconscious.Should().BeFalse();
            troll.IsDead.Should().BeFalse();

            // Debug: Can we find the axe?
            var foundByLocation = engine.Context.CurrentLocation.HasMatchingNoun("axe", true);
            foundByLocation.HasItem.Should().BeTrue("location should find axe via troll");
            foundByLocation.TheItem.Should().Be(axe);

            var foundByScope = Repository.GetItemInScope("axe", engine.Context);
            foundByScope.Should().Be(axe, "GetItemInScope should find the axe");

            // Act
            var result = await engine.GetResponse("take axe");

            // Assert
            result.Should().Contain("swings it out of your reach");
            engine.Context.HasItem<BloodyAxe>().Should().BeFalse();
        }

        [Test]
        public async Task TakeAxe_TrollUnconscious_ShouldSucceed()
        {
            // Arrange: Troll is unconscious
            var engine = GetTarget();
            engine.Context.CurrentLocation = Repository.GetLocation<ZorkOne.Location.TrollRoom>();

            var lantern = Repository.GetItem<Lantern>();
            lantern.IsOn = true;
            engine.Context.Take(lantern);

            var troll = Repository.GetItem<Troll>();
            troll.IsUnconscious = true;
            var axe = Repository.GetItem<BloodyAxe>();

            // Verify axe is still with troll but he's unconscious
            troll.ItemBeingHeld.Should().Be(axe);
            axe.CannotBeTakenDescription.Should().BeNull("unconscious troll can't defend the axe");

            // Act
            var result = await engine.GetResponse("take axe");

            // Assert
            result.Should().Contain("Taken");
            engine.Context.HasItem<BloodyAxe>().Should().BeTrue();
        }

        [Test]
        public async Task TakeAxe_TrollDead_ShouldSucceed()
        {
            // Arrange: Troll is dead
            var engine = GetTarget();
            engine.Context.CurrentLocation = Repository.GetLocation<ZorkOne.Location.TrollRoom>();

            var lantern = Repository.GetItem<Lantern>();
            lantern.IsOn = true;
            engine.Context.Take(lantern);

            var troll = Repository.GetItem<Troll>();
            troll.IsDead = true;
            var axe = Repository.GetItem<BloodyAxe>();

            // Verify axe is with dead troll
            troll.ItemBeingHeld.Should().Be(axe);

            // Act
            var result = await engine.GetResponse("take axe");

            // Assert
            result.Should().Contain("Taken");
            engine.Context.HasItem<BloodyAxe>().Should().BeTrue();
        }

        [Test]
        public async Task TakeAxe_NotInTrollRoom_CannotSeeAxe()
        {
            // Arrange: Player is in a different room
            var engine = GetTarget();
            engine.Context.CurrentLocation = Repository.GetLocation<ZorkOne.Location.Kitchen>();

            var troll = Repository.GetItem<Troll>();
            var axe = Repository.GetItem<BloodyAxe>();

            // Verify troll has axe but is in different room
            troll.ItemBeingHeld.Should().Be(axe);
            engine.Context.CurrentLocation.HasItem<Troll>().Should().BeFalse();

            // Act
            var result = await engine.GetResponse("take axe");

            // Assert
            result.Should().NotContain("Taken");
            result.Should().NotContain("swings it out of your reach");
            engine.Context.HasItem<BloodyAxe>().Should().BeFalse();
        }

        [Test]
        public async Task TakeAxe_AlreadyHaveIt_ShouldSayAlreadyHave()
        {
            // Arrange: Player already has the axe
            var engine = GetTarget();
            engine.Context.CurrentLocation = Repository.GetLocation<ZorkOne.Location.Kitchen>();

            var axe = Repository.GetItem<BloodyAxe>();
            engine.Context.Take(axe);

            // Verify player has it
            engine.Context.HasItem<BloodyAxe>().Should().BeTrue();

            // Act
            var result = await engine.GetResponse("take axe");

            // Assert
            result.Should().Contain("already have");
            engine.Context.Items.Count(i => i is BloodyAxe).Should().Be(1, "should only have one axe");
        }

        [Test]
        public async Task TakeAxe_OnGroundInTrollRoom_ShouldSucceed()
        {
            // Arrange: Axe is on the ground (troll doesn't have it)
            var engine = GetTarget();
            engine.Context.CurrentLocation = Repository.GetLocation<ZorkOne.Location.TrollRoom>();

            var lantern = Repository.GetItem<Lantern>();
            lantern.IsOn = true;
            engine.Context.Take(lantern);

            var troll = Repository.GetItem<Troll>();
            var axe = Repository.GetItem<BloodyAxe>();

            // Drop the axe on the ground
            troll.ItemBeingHeld = null;
            engine.Context.CurrentLocation.ItemPlacedHere(axe);

            // Verify axe is on ground, not with troll
            troll.ItemBeingHeld.Should().BeNull();
            axe.CurrentLocation.Should().Be(engine.Context.CurrentLocation);

            // Act
            var result = await engine.GetResponse("take axe");

            // Assert
            result.Should().Contain("Taken");
            engine.Context.HasItem<BloodyAxe>().Should().BeTrue();
        }

        [Test]
        public async Task TakeAxe_InDarkTrollRoom_ShouldSayTooDark()
        {
            // Arrange: In troll room but no light source
            var engine = GetTarget();
            engine.Context.CurrentLocation = Repository.GetLocation<ZorkOne.Location.TrollRoom>();

            // No light source - troll room is dark
            engine.Context.HasLightSource.Should().BeFalse();

            // Act
            var result = await engine.GetResponse("take axe");

            // Assert
            result.Should().Contain("too dark");
            engine.Context.HasItem<BloodyAxe>().Should().BeFalse();
        }
    }
}