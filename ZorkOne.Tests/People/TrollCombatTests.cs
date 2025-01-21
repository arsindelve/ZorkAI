using FluentAssertions;
using GameEngine;
using Moq;
using ZorkOne.ActorInteraction;
using ZorkOne.Interface;
using ZorkOne.Item;

namespace ZorkOne.Tests.People;

public class TrollCombatTests : EngineTestsBase
{
    [Test]
    public void Stunned()
    {
        var engine = GetTarget();
        var target = new TrollCombatEngine();
        Repository.GetItem<Troll>().IsStunned = true;

        var result = target.Attack(engine.Context);

        // Assert
        result.Should().Contain("The troll slowly regains his feet");
        Repository.GetItem<Troll>().IsStunned.Should().BeFalse();
    }

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
}