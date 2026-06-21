using FluentAssertions;
using GameEngine;
using Model.Interface;
using Moq;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Tests;

/// <summary>
///     Issue #190 — moving from one pitch-black room into another with no light source should get
///     you eaten by a grue. The original makes a dark->dark move an 80% chance of death
///     (zork1/gverbs.zil:2095). RoundRoom and NorthSouthPassage are adjacent dark rooms with no
///     special entry behavior, which makes them a clean fixture for the move case.
/// </summary>
[TestFixture]
public class GrueTests : EngineTestsBase
{
    private const string GrueDeath = "grue";
    private const string Devoured = "devoured";

    [TestFixture]
    public class MovingThroughDarkness : GrueTests
    {
        [Test]
        public async Task DarkToDark_NoLight_RollIsDeadly_GrueDevoursYou()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<RoundRoom>();
            target.Context.Chooser = Roll(50); // <= 80 => death

            var response = await target.GetResponse("north");

            response.Should().Contain(GrueDeath);
            response.Should().Contain(Devoured);
            target.Context.DeathCounter.Should().Be(1);
            target.Context.CurrentLocation.Should().Be(Repository.GetLocation<ForestOne>(),
                "death reincarnates the player in the forest");
        }

        [Test]
        public async Task DarkToDark_NoLight_RollMisses_YouStumbleThroughUnharmed()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<RoundRoom>();
            target.Context.Chooser = Roll(81); // > 80 => survive

            var response = await target.GetResponse("north");

            response.Should().NotContain(Devoured);
            target.Context.DeathCounter.Should().Be(0);
            target.Context.CurrentLocation.Should().Be(Repository.GetLocation<NorthSouthPassage>());
        }

        // The 80% threshold from <PROB 80>: a roll of 80 is still deadly; 81 is the first safe roll.
        [TestCase(1, true)]
        [TestCase(80, true)]
        [TestCase(81, false)]
        [TestCase(100, false)]
        public async Task DarkToDark_DeathProbabilityBoundaryIsEighty(int roll, bool shouldDie)
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<RoundRoom>();
            target.Context.Chooser = Roll(roll);

            await target.GetResponse("north");

            if (shouldDie)
            {
                target.Context.DeathCounter.Should().Be(1);
                target.Context.CurrentLocation.Should().Be(Repository.GetLocation<ForestOne>());
            }
            else
            {
                target.Context.DeathCounter.Should().Be(0);
                target.Context.CurrentLocation.Should().Be(Repository.GetLocation<NorthSouthPassage>());
            }
        }

        [Test]
        public async Task DarkToDark_WithALitLamp_TheGrueNeverGetsAChance()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<RoundRoom>();
            var lamp = Repository.GetItem<Lantern>();
            lamp.IsOn = true;
            target.Context.Take(lamp);
            var chooser = new Mock<IRandomChooser>();
            target.Context.Chooser = chooser.Object;

            await target.GetResponse("north");

            target.Context.DeathCounter.Should().Be(0);
            target.Context.CurrentLocation.Should().Be(Repository.GetLocation<NorthSouthPassage>());
            chooser.Verify(c => c.RollDice(It.IsAny<int>()), Times.Never,
                "with light there is no darkness, so the grue check must short-circuit before rolling");
        }
    }

    [TestFixture]
    public class WhenTheGrueShouldNotStrike : GrueTests
    {
        // Stepping from a lit room into your first dark room: you stumble in, warned but alive.
        [Test]
        public void FirstStepFromLightIntoDarkness_IsSafe()
        {
            var target = GetTarget();
            var chooser = new Mock<IRandomChooser>();
            target.Context.Chooser = chooser.Object;

            target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>(); // lit
            target.Context.ProcessBeginningOfTurn();
            target.Context.CurrentLocation = Repository.GetLocation<RoundRoom>(); // moved into the dark
            var result = target.Context.ProcessEndOfTurn();

            result.Should().BeNull();
            target.Context.DeathCounter.Should().Be(0);
            chooser.Verify(c => c.RollDice(It.IsAny<int>()), Times.Never);
        }

        // Standing still in the dark (a non-move turn) does not move you, so no grue.
        [Test]
        public void StayingPutInTheDark_IsSafe()
        {
            var target = GetTarget();
            var chooser = new Mock<IRandomChooser>();
            target.Context.Chooser = chooser.Object;

            target.Context.CurrentLocation = Repository.GetLocation<RoundRoom>(); // dark
            target.Context.ProcessBeginningOfTurn();
            // no movement this turn
            var result = target.Context.ProcessEndOfTurn();

            result.Should().BeNull();
            target.Context.DeathCounter.Should().Be(0);
            chooser.Verify(c => c.RollDice(It.IsAny<int>()), Times.Never);
        }

        // Escaping the dark into the light is safe: the destination must also be dark.
        [Test]
        public void MovingFromDarknessIntoTheLight_IsSafe()
        {
            var target = GetTarget();
            var chooser = new Mock<IRandomChooser>();
            target.Context.Chooser = chooser.Object;

            target.Context.CurrentLocation = Repository.GetLocation<RoundRoom>(); // dark
            target.Context.ProcessBeginningOfTurn();
            target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>(); // moved into light
            var result = target.Context.ProcessEndOfTurn();

            result.Should().BeNull();
            target.Context.DeathCounter.Should().Be(0);
            chooser.Verify(c => c.RollDice(It.IsAny<int>()), Times.Never);
        }
    }

    private static IRandomChooser Roll(int value)
    {
        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(c => c.RollDice(100)).Returns(value);
        return chooser.Object;
    }
}
