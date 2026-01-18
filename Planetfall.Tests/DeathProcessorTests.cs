using FluentAssertions;
using Model.Interaction;
using Planetfall.Command;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

/// <summary>
/// Tests for the DeathProcessor and game restart mechanism.
/// </summary>
public class DeathProcessorTests : EngineTestsBase
{
    [TestFixture]
    public class ProcessMethod : DeathProcessorTests
    {
        [Test]
        public void Should_ReturnDeathInteractionResult()
        {
            GetTarget();
            var context = new PlanetfallContext();
            var processor = new DeathProcessor();

            var result = processor.Process("You died.", context);

            result.Should().BeOfType<DeathInteractionResult>();
        }

        [Test]
        public void Should_IncrementDeathCounter()
        {
            GetTarget();
            var context = new PlanetfallContext();
            context.DeathCounter.Should().Be(0);

            var processor = new DeathProcessor();
            processor.Process("You died.", context);

            context.DeathCounter.Should().Be(1);
        }

        [Test]
        public void Should_IncrementDeathCounter_OnMultipleDeaths()
        {
            GetTarget();
            var context = new PlanetfallContext();
            var processor = new DeathProcessor();

            processor.Process("First death.", context);
            processor.Process("Second death.", context);
            processor.Process("Third death.", context);

            context.DeathCounter.Should().Be(3);
        }

        [Test]
        public void Should_SetPendingDeath_OnContext()
        {
            GetTarget();
            var context = new PlanetfallContext();
            context.PendingDeath.Should().BeNull();

            var processor = new DeathProcessor();
            var result = processor.Process("You died.", context);

            context.PendingDeath.Should().NotBeNull();
            context.PendingDeath.Should().BeSameAs(result);
        }

        [Test]
        public void Should_IncludeDeathMessage_InResult()
        {
            GetTarget();
            var context = new PlanetfallContext();
            var processor = new DeathProcessor();

            var result = processor.Process("You fell into a pit.", context);

            result.InteractionMessage.Should().Contain("You fell into a pit.");
        }

        [Test]
        public void Should_IncludeDeathBanner_InResult()
        {
            GetTarget();
            var context = new PlanetfallContext();
            var processor = new DeathProcessor();

            var result = processor.Process("You died.", context);

            result.InteractionMessage.Should().Contain("*** You have died ***");
        }

        [Test]
        public void Should_IncludeTreatyOfGishenIV_InResult()
        {
            GetTarget();
            var context = new PlanetfallContext();
            var processor = new DeathProcessor();

            var result = processor.Process("You died.", context);

            result.InteractionMessage.Should().Contain("Treaty of Gishen IV");
        }

        [Test]
        public void Should_IncludeScore_InResult()
        {
            GetTarget();
            var context = new PlanetfallContext();
            context.AddPoints(25);
            var processor = new DeathProcessor();

            var result = processor.Process("You died.", context);

            result.InteractionMessage.Should().Contain("25");
        }

        [Test]
        public void Should_ReturnCorrectDeathCount_InResult()
        {
            GetTarget();
            var context = new PlanetfallContext();
            context.DeathCounter = 5;
            var processor = new DeathProcessor();

            var result = processor.Process("You died.", context);

            result.DeathCount.Should().Be(6); // Incremented to 6
        }
    }

    [TestFixture]
    public class DeathInteractionResultTests : DeathProcessorTests
    {
        [Test]
        public void Should_HaveInteractionHappened_True()
        {
            var result = new DeathInteractionResult("message", 1);
            result.InteractionHappened.Should().BeTrue();
        }

        [Test]
        public void Should_StoreMessage()
        {
            var result = new DeathInteractionResult("Test death message", 1);
            result.InteractionMessage.Should().Be("Test death message");
        }

        [Test]
        public void Should_StoreDeathCount()
        {
            var result = new DeathInteractionResult("message", 42);
            result.DeathCount.Should().Be(42);
        }
    }

    [TestFixture]
    public class ContextDeathMethods : DeathProcessorTests
    {
        [Test]
        public void GetDeathCount_Should_ReturnDeathCounter()
        {
            GetTarget();
            var context = new PlanetfallContext();
            context.DeathCounter = 7;

            context.GetDeathCount().Should().Be(7);
        }

        [Test]
        public void SetDeathCount_Should_UpdateDeathCounter()
        {
            GetTarget();
            var context = new PlanetfallContext();
            context.DeathCounter = 0;

            context.SetDeathCount(15);

            context.DeathCounter.Should().Be(15);
        }
    }

    [TestFixture]
    public class GameEngineDeathHandling : DeathProcessorTests
    {
        [Test]
        public async Task Should_ResetGameState_WhenPlayerDies()
        {
            var engine = GetTarget();
            StartHere<MessHall>();

            // Collect some items and change state
            Take<Brush>();
            engine.Context.AddPoints(10);
            engine.Context.Moves = 50;

            // Trigger death via hunger
            engine.Context.Hunger = HungerLevel.AboutToPassOut;
            engine.Context.HungerNotifications.NextWarningAt = engine.Context.CurrentTime;

            // Process turn to trigger death
            await engine.GetResponse("wait");

            // After death, game should be reset
            engine.Context.Score.Should().Be(0);
            engine.Context.Moves.Should().Be(0);
            engine.Context.CurrentLocation.Should().BeOfType<DeckNine>();
        }

        [Test]
        public async Task Should_PreserveDeathCounter_WhenPlayerDies()
        {
            var engine = GetTarget();
            StartHere<MessHall>();

            // Set initial death counter
            engine.Context.DeathCounter = 3;

            // Trigger death via hunger
            engine.Context.Hunger = HungerLevel.AboutToPassOut;
            engine.Context.HungerNotifications.NextWarningAt = engine.Context.CurrentTime;

            // Process turn to trigger death
            await engine.GetResponse("wait");

            // Death counter should be incremented (3 -> 4)
            engine.Context.DeathCounter.Should().Be(4);
        }

        [Test]
        public async Task Should_ReturnDeathMessage_WhenPlayerDies()
        {
            var engine = GetTarget();
            StartHere<MessHall>();

            // Trigger death via hunger
            engine.Context.Hunger = HungerLevel.AboutToPassOut;
            engine.Context.HungerNotifications.NextWarningAt = engine.Context.CurrentTime;

            var result = await engine.GetResponse("wait");

            result.Should().Contain("*** You have died ***");
            result.Should().Contain("Treaty of Gishen IV");
        }

        [Test]
        public async Task Should_ShowNewLocationDescription_AfterDeath()
        {
            var engine = GetTarget();
            StartHere<MessHall>();

            // Trigger death via hunger
            engine.Context.Hunger = HungerLevel.AboutToPassOut;
            engine.Context.HungerNotifications.NextWarningAt = engine.Context.CurrentTime;

            var result = await engine.GetResponse("wait");

            // Should contain the starting location description
            result.Should().Contain("Deck Nine");
        }

        [Test]
        public async Task Should_ResetInventory_WhenPlayerDies()
        {
            var engine = GetTarget();
            StartHere<MessHall>();

            // Add extra items beyond what we start with
            var initialCount = engine.Context.Items.Count;
            Take<SurvivalKit>();
            engine.Context.Items.Count.Should().Be(initialCount + 1);

            // Trigger death via hunger
            engine.Context.Hunger = HungerLevel.AboutToPassOut;
            engine.Context.HungerNotifications.NextWarningAt = engine.Context.CurrentTime;

            await engine.GetResponse("wait");

            // After death, inventory should be reset (SurvivalKit should be gone)
            engine.Context.Items.Should().NotContain(i => i.GetType() == typeof(SurvivalKit));
        }

        [Test]
        public async Task Should_ResetDay_WhenPlayerDies()
        {
            var engine = GetTarget();
            StartHere<MessHall>();
            engine.Context.Day = 5;

            // Trigger death via hunger
            engine.Context.Hunger = HungerLevel.AboutToPassOut;
            engine.Context.HungerNotifications.NextWarningAt = engine.Context.CurrentTime;

            await engine.GetResponse("wait");

            engine.Context.Day.Should().Be(1);
        }

        [Test]
        public async Task Should_ResetHunger_WhenPlayerDies()
        {
            var engine = GetTarget();
            StartHere<MessHall>();

            // Trigger death via hunger
            engine.Context.Hunger = HungerLevel.AboutToPassOut;
            engine.Context.HungerNotifications.NextWarningAt = engine.Context.CurrentTime;

            await engine.GetResponse("wait");

            engine.Context.Hunger.Should().Be(HungerLevel.WellFed);
        }

        [Test]
        public async Task Should_ResetTired_WhenPlayerDies()
        {
            var engine = GetTarget();
            StartHere<MessHall>();
            engine.Context.Tired = TiredLevel.Exhausted;

            // Trigger death via hunger
            engine.Context.Hunger = HungerLevel.AboutToPassOut;
            engine.Context.HungerNotifications.NextWarningAt = engine.Context.CurrentTime;

            await engine.GetResponse("wait");

            engine.Context.Tired.Should().Be(TiredLevel.WellRested);
        }

        [Test]
        public async Task Should_ClearPendingDeath_AfterRestart()
        {
            var engine = GetTarget();
            StartHere<MessHall>();

            // Trigger death via hunger
            engine.Context.Hunger = HungerLevel.AboutToPassOut;
            engine.Context.HungerNotifications.NextWarningAt = engine.Context.CurrentTime;

            await engine.GetResponse("wait");

            // PendingDeath should be cleared after restart (new context has null)
            engine.Context.PendingDeath.Should().BeNull();
        }
    }
}
