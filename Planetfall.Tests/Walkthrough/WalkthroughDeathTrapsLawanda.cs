using GameEngine;
using JetBrains.Annotations;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Computer;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Tests.Walkthrough;

// Point traps, Lawanda side and inside the computer. Same convention as
// WalkthroughDeathTrapsKalamontee.cs: a god-mode drop next to the trap, then the last few moves
// played normally.
//
// The bio lab, the fungicide window and the mutant chase are already covered end to end by
// WalkthroughMutantChase and WalkthroughBioLock - every way of being eaten, in every room, plus the
// cryo-elevator button. Nothing here repeats them.

/// <summary>
///     The radiation lab is behind a decontamination chamber with a sign reading "raadeeaashun suuts
///     must bee worn beeyond xis point", and the canisters inside are split open. There is no radiation
///     suit in the game. The room gives you six turns and no way to undo them.
/// </summary>
[TestFixture]
public sealed class WalkthroughStayInTheRadiationLab : WalkthroughTestBase
{
    [UsedImplicitly]
    public void AtTheRadiationLock()
    {
        StartHere<RadiationLockEast>();
        Repository.GetItem<RadiationLockOuterDoor>().IsOpen = false;
    }

    [Test]
    [TestCase("look", "AtTheRadiationLock", "eastern half of the decontamination chamber",
        "Raadeeaashun suuts must bee worn beeyond xis point")]
    [TestCase("open radiation-lock door", null, "The door opens")]
    [TestCase("east", null, "Radiation Lab", "large canisters bearing radioactive warnings",
        "Many of the canisters are split open")]
    // There is nothing here you need. The brown spool and the lamp are worth grabbing and leaving;
    // this walkthrough browses instead.
    [TestCase("examine equipment", null, "so complicated that you couldn't even begin to figure out how to operate it")]
    [TestCase("look through crack", null, "You see a dimly lit Bio Lab", "Sinister shapes lurk about within")]
    [TestCase("wait", null, "Time passes")]
    // The dose has already been taken by the time the game mentions it.
    [TestCase("wait", null, "You suddenly feel sick and dizzy")]
    [TestCase("west", null, "You feel incredibly nauseous and begin vomiting", "all your hair has fallen out")]
    // Leaving does not help. The counter is in you, not in the room.
    [TestCase("west", null,
        "It seems you have picked up a bad case of radiation poisoning",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The miniaturization booth shrinks you and fires you into a numbered sector of the ship's
///     computer. Sector 384 is the dead one you are there to repair. Every other number you can type is
///     a live circuit.
/// </summary>
[TestFixture]
public sealed class WalkthroughTypeTheWrongSectorNumber : WalkthroughTestBase
{
    [UsedImplicitly]
    public void InTheMiniaturizationBooth()
    {
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();
    }

    [Test]
    [TestCase("slide mini card through slot", "InTheMiniaturizationBooth",
        "Miniaturization and teleportation booth activated", "Please type in damaged sector number")]
    [TestCase("examine keyboard", null, "a simple numeric keypad, its ten keys numbered zero through nine")]
    // "Damaged sector number" is doing a lot of work in that recording. 385 is not damaged.
    [TestCase("type 385", null,
        "You seem to have transported yourself into an active sector of the computer",
        "You are fried by powerful electric currents",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     With the speck burned away, an elephant-sized microbe drops onto the silicon strip and blocks
///     the way back to the station. It closes in a little further every turn you do not fight it, and
///     on the third it stops closing.
/// </summary>
[TestFixture]
public sealed class WalkthroughStandStillWhileTheMicrobeCloses : WalkthroughTestBase
{
    [UsedImplicitly]
    public void OnTheStripWithTheComputerFixed()
    {
        StartHere<StripNearRelay>();
        Repository.GetItem<Relay>().SpeckDestroyed = true;
    }

    [Test]
    [TestCase("south", "OnTheStripWithTheComputerFixed", "Middle of Strip",
        "giant elephant-sized monster lands on the strip")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The intended solution is to heat the laser up and get rid of it - the microbe goes after the
///     warmth. Heat it far enough and keep hold of it, though, and the microbe comes after the warmth
///     while you are still attached to it.
/// </summary>
[TestFixture]
public sealed class WalkthroughHoldTheOverheatedLaserWhenTheMicrobeLunges : WalkthroughTestBase
{
    [UsedImplicitly]
    public void OnTheStripWithAHotLaser()
    {
        StartHere<StripNearRelay>();
        Repository.GetItem<Relay>().SpeckDestroyed = true;

        var laser = Repository.GetItem<Laser>();
        laser.Items.Clear();
        var battery = Repository.GetItem<FreshBattery>();
        battery.ChargesRemaining = 50;
        laser.ItemPlacedHere(battery);
        Take<Laser>();

        // Fourteen shots' worth of accumulated heat. Firing them one at a time would be the same
        // walkthrough with thirteen more identical lines in it; the interesting move is the last one.
        laser.WarmthLevel = 14;
    }

    [Test]
    [TestCase("south", "OnTheStripWithAHotLaser", "Middle of Strip",
        "giant elephant-sized monster lands on the strip")]
    [TestCase("set laser to 2", null, "The dial is now set to 2.")]
    // One more shot from a laser this hot, still in your hands when the microbe reacts to it.
    [TestCase("shoot microbe with laser", null, "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     Destroying the speck restores the sector, and the voice that whispers "Sector 384 will activate
///     in 200 millichrons. Proceed to exit station" is not being decorative. The strip you are standing
///     on is a circuit, and it is about to have current in it.
/// </summary>
[TestFixture]
public sealed class WalkthroughStayOnTheStripUntilTheSectorPowersUp : WalkthroughTestBase
{
    [UsedImplicitly]
    public void OnTheStripWithALaser()
    {
        StartHere<StripNearRelay>();

        var laser = Repository.GetItem<Laser>();
        laser.Items.Clear();
        var battery = Repository.GetItem<FreshBattery>();
        battery.ChargesRemaining = 50;
        laser.ItemPlacedHere(battery);
        Take<Laser>();
    }

    /// <summary>
    ///     Fast-forwards the 200-millichron countdown to its last few turns. The walkthrough is about
    ///     what happens when it runs out, not about typing "wait" two hundred times.
    /// </summary>
    [UsedImplicitly]
    public void TheSectorIsAboutToPowerUp()
    {
        Repository.GetItem<SectorActivationTimer>().TurnsRemaining = 3;
    }

    [Test]
    [TestCase("set laser to 1", "OnTheStripWithALaser", "The dial is now set to 1.")]
    [TestCase("shoot speck with laser", null, "The speck is hit by the beam! It sizzles a little, but isn't destroyed yet.")]
    [TestCase("shoot speck with laser", null,
        "This time, it vaporizes into a fine cloud of ash",
        "Sector 384 will activate in 200 millichrons",
        "Proceed to exit station")]
    [TestCase("wait", "TheSectorIsAboutToPowerUp", "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null,
        "The computer comes back to life with a surge of electric current",
        "you are still standing on one of its circuits",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}
