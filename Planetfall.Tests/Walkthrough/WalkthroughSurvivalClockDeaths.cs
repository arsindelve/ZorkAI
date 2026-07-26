using GameEngine;
using JetBrains.Annotations;
using Model.Interface;
using Moq;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Dorm;

namespace Planetfall.Tests.Walkthrough;

// The three clocks that kill you if you ignore them: sleep, hunger/thirst, and the disease.
//
// None of these is a puzzle you can fail - they are all just the passage of time, and every one of
// them has a fix that is freely available. Sleep in a bunk. Eat and drink from the kitchen. Take the
// medicine. Each fixture below plays the last few turns of a clock that was never attended to.
//
// The clocks run in real game time - hundreds of ticks between levels - so these fast-forward the
// timer with a god-mode step and then play the consequence normally, rather than typing "wait" four
// hundred times.

/// <summary>
///     Sleeping where it floods. The crag you landed on is dry on the first day and underwater by the
///     second; the ocean does not wait for you to wake up. This is the version of the trap the game is
///     least likely to warn you about, because you do not choose to lie down - fatigue chooses for you,
///     wherever you happen to be standing.
/// </summary>
[TestFixture]
public sealed class WalkthroughFallAsleepOnTheCragAndDrown : WalkthroughTestBase
{
    [UsedImplicitly]
    public void ExhaustedOnTheCrag()
    {
        StartHere<Crag>();
        Context.Day = 1;
    }

    /// <summary>
    ///     Winds the fatigue clock to its last notch. In a real game this is thousands of ticks of
    ///     walking around; the walkthrough is about where you are standing when it runs out.
    /// </summary>
    [UsedImplicitly]
    public void AboutToDropWhereYouStand()
    {
        Context.Tired = TiredLevel.AboutToDrop;
        Context.SleepNotifications.NextWarningAt = Context.CurrentTime;
    }

    [Test]
    [TestCase("look", "ExhaustedOnTheCrag", "Crag")]
    // A bunk is up the cliff, through the courtyard and along two corridors. It is not close, and
    // this is the last turn on which starting for it would have mattered.
    // The engine surfaces the death itself rather than the "you drop to the ground" preamble that
    // precedes it, so that is what the walkthrough asserts on.
    [TestCase("wait", "AboutToDropWhereYouStand",
        "in the middle of the night, a wave of water washes over you",
        "Before you can quite get your bearings, you drown",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The same forced sleep, on dry land. Sleeping rough anywhere outside a dormitory is a one-in-
///     three chance of not waking up; something in the dark finds you, and the game is not certain it
///     was a grue.
/// </summary>
[TestFixture]
public sealed class WalkthroughFallAsleepInTheOpenAndBeEaten : WalkthroughTestBase
{
    [UsedImplicitly]
    public void ExhaustedInTheCourtyard()
    {
        StartHere<Courtyard>();

        // The beast attack is a 30-in-100 roll inside SleepEngine, whose chooser is static. Pin it to
        // the attack and put it back in OneTimeTearDown so no later fixture inherits the mock.
        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(s => s.RollDice(100)).Returns(1);
        SleepEngine.Chooser = chooser.Object;

        Context.Tired = TiredLevel.AboutToDrop;
        Context.SleepNotifications.NextWarningAt = Context.CurrentTime;
    }

    [OneTimeTearDown]
    public void RestoreTheSleepChooser()
    {
        SleepEngine.Chooser = new RandomChooser();
    }

    [Test]
    [TestCase("wait", "ExhaustedInTheCourtyard",
        "several ferocious beasts",
        "could they be grues?",
        "Perhaps you should have found a slightly safer place to sleep",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     Hunger and thirst. There is a kitchen behind the mess hall with a machine that dispenses food
///     and a canteen you can fill, and the game escalates through four separate warnings before it
///     kills you. This walkthrough reads all four and does nothing about any of them.
/// </summary>
[TestFixture]
public sealed class WalkthroughStarveToDeath : WalkthroughTestBase
{
    [UsedImplicitly]
    public void InTheDorm()
    {
        StartHere<DormA>();
    }

    /// <summary>
    ///     Brings the hunger clock's next tick forward to now. Called once per rung of the ladder so
    ///     the walkthrough can show all four warnings without four hundred turns of walking.
    /// </summary>
    [UsedImplicitly]
    public void SkipToTheNextPangOfHunger()
    {
        Context.HungerNotifications.NextWarningAt = Context.CurrentTime;
    }

    [Test]
    [TestCase("look", "InTheDorm", "Dorm")]
    [TestCase("wait", "SkipToTheNextPangOfHunger",
        "A growl from your stomach warns that you're getting pretty hungry and thirsty")]
    [TestCase("wait", "SkipToTheNextPangOfHunger",
        "You're now really ravenous and your lips are quite parched")]
    [TestCase("wait", "SkipToTheNextPangOfHunger",
        "You're starting to feel faint from lack of food and liquid")]
    // The last warning says exactly what happens next, and it is still not too late at this point.
    [TestCase("wait", "SkipToTheNextPangOfHunger",
        "If you don't eat or drink something in a few millichrons, you'll probably pass out")]
    [TestCase("wait", "SkipToTheNextPangOfHunger",
        "You collapse from extreme thirst and hunger",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The disease. It is the clock the whole game is built around - the reason you are fixing the
///     computer at all - and it advances one level every time you sleep, whatever else you did that
///     day. There is medicine in the infirmary that pushes the counter back down. Untreated, the ninth
///     day is the last one, and you die in your sleep rather than at any moment you could have acted on.
/// </summary>
[TestFixture]
public sealed class WalkthroughSleepThroughTheDiseaseUntilItKillsYou : WalkthroughTestBase
{
    [UsedImplicitly]
    public void EightDaysIll()
    {
        StartHere<DormA>();

        // Eight nights of an untreated infection. The medicine bottle in the infirmary is still full.
        Context.SicknessCounter = 8;

        Context.Tired = TiredLevel.AboutToDrop;
        Context.SleepNotifications.NextWarningAt = Context.CurrentTime;
    }

    [Test]
    // Forced sleep in a dormitory is the safe kind - you climb into a bunk instead of dropping where
    // you stand. It makes no difference tonight.
    [TestCase("wait", "EightDaysIll",
        "You finally succumb to the ravages of your illness and collapse",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}
