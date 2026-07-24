using FluentAssertions;
using GameEngine;
using JetBrains.Annotations;
using Stationfall.Item.Duffy;

namespace Stationfall.Tests.Walkthrough;

/// <summary>
///     Drives a documented walkthrough through the real engine, one command per <c>[TestCase]</c> row,
///     asserting on progression checkpoints (room names, score, key state) rather than on exact prose.
///     Mirrors <c>Planetfall.Tests.Walkthrough.WalkthroughTestBase</c>.
///     The engine is built once per fixture and the rows run in declaration order, so each row picks up
///     where the previous one left off.
/// </summary>
public abstract class WalkthroughTestBase : EngineTestsBase
{
    private GameEngine<StationfallGame, StationfallContext> _target = null!;

    [OneTimeSetUp]
    public void Init()
    {
        _target = GetTarget();
    }

    /// <summary>
    ///     Pins the chronometer so the autopilot course is reproducible. The correct heading is derived
    ///     from this reading, so a walkthrough can only name a fixed course if the clock is fixed too.
    ///     Any reading from 6600 to 6699 yields course 103.
    /// </summary>
    [UsedImplicitly]
    public void PinChronometer()
    {
        Repository.GetItem<Chronometer>().CurrentTime = WalkthroughShipDeparture.PinnedTime;
    }

    protected void InvokeSetup(string setup)
    {
        var method = GetType().GetMethod(setup)
                     ?? throw new ArgumentException($"Setup method '{setup}' doesn't exist. ");

        method.Invoke(this, null);
    }

    protected async Task Do(string input, params string[] expectedResponses)
    {
        var result = await _target.GetResponse(input);

        foreach (var expected in expectedResponses)
            result.Should().Contain(expected, $"the walkthrough command \"{input}\" should produce it");
    }

    protected StationfallContext Ctx => _target.Context;
}
