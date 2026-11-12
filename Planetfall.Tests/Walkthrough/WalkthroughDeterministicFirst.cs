using GameEngine;
using Model.Interface;
using UnitTests;
using Planetfall.GlobalCommand;

namespace Planetfall.Tests.Walkthrough;

/// <summary>
///     Every walkthrough in this project, replayed with the production <see cref="DeterministicParser" />
///     layered ahead of the test parser (see <see cref="DeterministicFirstTestParser" />). The base
///     fixtures prove the game's handlers; these prove that when the grammar answers a command, the engine
///     does exactly what it does for the parse the walkthrough was written against - across every
///     route, including the deaths and dead ends, where the unusual commands are.
/// </summary>
internal static class DeterministicFirst
{
    public static IIntentParser Parser() => new DeterministicFirstTestParser(new PlanetfallGlobalCommandFactory(), "Planetfall");
}

[TestFixture]
public sealed class WalkthroughBioLockDeterministicFirst : WalkthroughBioLock
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughByLocationNameDeterministicFirst : WalkthroughByLocationName
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughLingerInTheUndertowDeterministicFirst : WalkthroughLingerInTheUndertow
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughSwimBackDownIntoTheUndertowDeterministicFirst : WalkthroughSwimBackDownIntoTheUndertow
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughJumpIntoTheRiftDeterministicFirst : WalkthroughJumpIntoTheRift
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughDrinkTheChemicalsDeterministicFirst : WalkthroughDrinkTheChemicals
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughCrashTheShuttleIntoTheStationDeterministicFirst : WalkthroughCrashTheShuttleIntoTheStation
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughTouchTheLiveBedistorDeterministicFirst : WalkthroughTouchTheLiveBedistor
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughStayInTheRadiationLabDeterministicFirst : WalkthroughStayInTheRadiationLab
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughTypeTheWrongSectorNumberDeterministicFirst : WalkthroughTypeTheWrongSectorNumber
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughStandStillWhileTheMicrobeClosesDeterministicFirst : WalkthroughStandStillWhileTheMicrobeCloses
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughHoldTheOverheatedLaserWhenTheMicrobeLungesDeterministicFirst : WalkthroughHoldTheOverheatedLaserWhenTheMicrobeLunges
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughStayOnTheStripUntilTheSectorPowersUpDeterministicFirst : WalkthroughStayOnTheStripUntilTheSectorPowersUp
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughDontFixAnythingDeterministicFirst : WalkthroughDontFixAnything
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughMutantChaseDeterministicFirst : WalkthroughMutantChase
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughFallAsleepOnTheCragAndDrownDeterministicFirst : WalkthroughFallAsleepOnTheCragAndDrown
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughFallAsleepInTheOpenAndBeEatenDeterministicFirst : WalkthroughFallAsleepInTheOpenAndBeEaten
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughStarveToDeathDeterministicFirst : WalkthroughStarveToDeath
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughSleepThroughTheDiseaseUntilItKillsYouDeterministicFirst : WalkthroughSleepThroughTheDiseaseUntilItKillsYou
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughTestOneDeterministicFirst : WalkthroughTestOne
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughNeverEnterTheEscapePodDeterministicFirst : WalkthroughNeverEnterTheEscapePod
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughStrandedAwayFromTheEscapePodDeterministicFirst : WalkthroughStrandedAwayFromTheEscapePod
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughNeverSitInTheSafetyWebbingDeterministicFirst : WalkthroughNeverSitInTheSafetyWebbing
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughThrownAgainstTheBulkheadHeadFirstDeterministicFirst : WalkthroughThrownAgainstTheBulkheadHeadFirst
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughStandUpInTheLandedPodAndDrownDeterministicFirst : WalkthroughStandUpInTheLandedPodAndDrown
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughEatTheAmbassadorsCeleryDeterministicFirst : WalkthroughEatTheAmbassadorsCelery
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughPickAFightWithBlatherDeterministicFirst : WalkthroughPickAFightWithBlather
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughGetThrownInTheBrigAndBlowUpWithTheShipDeterministicFirst : WalkthroughGetThrownInTheBrigAndBlowUpWithTheShip
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughThrowTheMagnetIntoTheRiftDeterministicFirst : WalkthroughThrowTheMagnetIntoTheRift
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughBridgeTheRiftWithACollapsedLadderDeterministicFirst : WalkthroughBridgeTheRiftWithACollapsedLadder
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughCollapseTheLadderFromTheFarSideOfTheRiftDeterministicFirst : WalkthroughCollapseTheLadderFromTheFarSideOfTheRift
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughCarryTheMagnetWithTheAccessCardsDeterministicFirst : WalkthroughCarryTheMagnetWithTheAccessCards
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughSleepInTheInfirmaryBedDeterministicFirst : WalkthroughSleepInTheInfirmaryBed
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughLetFloydDieWithoutTheCardDeterministicFirst : WalkthroughLetFloydDieWithoutTheCard
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughRunTheLaserBatteryFlatBeforeTheSpeckDeterministicFirst : WalkthroughRunTheLaserBatteryFlatBeforeTheSpeck
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughLoseTheGoodBedistorInTheRiftDeterministicFirst : WalkthroughLoseTheGoodBedistorInTheRift
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}
