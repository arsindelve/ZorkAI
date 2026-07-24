using FluentAssertions;
using GameEngine;
using Stationfall.Item.Duffy;
using Stationfall.Location.Duffy;

namespace Stationfall.Tests;

[TestFixture]
public class TempReviewProbe : EngineTestsBase
{
    private const int PinnedTime = 6600;
    private const int ExpectedCourse = 103;

    private static async Task WalkToTruckWithFloyd(GameEngine<StationfallGame, StationfallContext> engine)
    {
        await engine.GetResponse("east");
        await engine.GetResponse("north");
        await engine.GetResponse("put authorization in slot");
        await engine.GetResponse("type 3");
        await engine.GetResponse("south");
        await engine.GetResponse("east");
        await engine.GetResponse("open hatch");
        await engine.GetResponse("in");
    }

    [Test]
    public async Task Probe_StrandedPlayer_CanReEnterAndIsTrapped()
    {
        var engine = GetTarget();
        await WalkToTruckWithFloyd(engine);
        await engine.GetResponse("sit in pilot seat");
        await engine.GetResponse("put activation in slot");
        Repository.GetItem<Chronometer>().CurrentTime = PinnedTime;
        var set = await engine.GetResponse($"type {ExpectedCourse}");
        TestContext.Out.WriteLine("SET: " + set);

        // Walk away from the truck entirely before liftoff.
        TestContext.Out.WriteLine("OUT: " + await engine.GetResponse("out"));
        TestContext.Out.WriteLine("W:   " + await engine.GetResponse("west"));
        TestContext.Out.WriteLine("N:   " + await engine.GetResponse("north"));

        for (var i = 0; i < 6; i++)
            TestContext.Out.WriteLine($"WAIT{i}: " + await engine.GetResponse("wait"));

        var truck = Repository.GetLocation<Spacetruck>();
        TestContext.Out.WriteLine($"LaunchCounter={truck.LaunchCounter} HasDocked={truck.HasDocked} InFlight={truck.IsInFlight}");
        TestContext.Out.WriteLine("LOC: " + engine.Context.CurrentLocation.Name);

        // Walk back to the truck and try to get in and out again.
        TestContext.Out.WriteLine("S:   " + await engine.GetResponse("south"));
        TestContext.Out.WriteLine("E:   " + await engine.GetResponse("east"));
        TestContext.Out.WriteLine("IN:  " + await engine.GetResponse("in"));
        TestContext.Out.WriteLine("LOC2: " + engine.Context.CurrentLocation.Name);
        TestContext.Out.WriteLine("OUT2:" + await engine.GetResponse("out"));
        TestContext.Out.WriteLine("LOC3: " + engine.Context.CurrentLocation.Name);
        TestContext.Out.WriteLine("TYPE:" + await engine.GetResponse("type 103"));
        TestContext.Out.WriteLine("OPEN:" + await engine.GetResponse("open hatch"));
    }

    [Test]
    public async Task Probe_TypeZero_Stalls()
    {
        var engine = GetTarget();
        await WalkToTruckWithFloyd(engine);
        await engine.GetResponse("close hatch");
        await engine.GetResponse("sit in pilot seat");
        await engine.GetResponse("put activation in slot");
        Repository.GetItem<Chronometer>().CurrentTime = PinnedTime;
        TestContext.Out.WriteLine("TYPE0: " + await engine.GetResponse("type 0"));

        var log = string.Empty;
        for (var i = 0; i < 12; i++)
            log += await engine.GetResponse("wait");

        TestContext.Out.WriteLine("LOG: " + log);
        var truck = Repository.GetLocation<Spacetruck>();
        TestContext.Out.WriteLine($"CoursePicked={truck.CoursePicked} LaunchCounter={truck.LaunchCounter} TurnsUntilLiftoff={truck.TurnsUntilLiftoff}");
    }

    [Test]
    public async Task Probe_FloydSeatFlagGoesStale()
    {
        var engine = GetTarget();
        await WalkToTruckWithFloyd(engine);
        TestContext.Out.WriteLine("SIT: " + await engine.GetResponse("sit in pilot seat"));
        TestContext.Out.WriteLine("STAND: " + await engine.GetResponse("stand"));
        TestContext.Out.WriteLine("OUT: " + await engine.GetResponse("out"));
        TestContext.Out.WriteLine("FloydLoc=" + Repository.GetItem<Floyd>().CurrentLocation?.Name);
        TestContext.Out.WriteLine("CopilotOccupiedByFloyd=" + Repository.GetItem<CopilotSeat>().OccupiedByFloyd);
        TestContext.Out.WriteLine("IN: " + await engine.GetResponse("in"));
        TestContext.Out.WriteLine("SIT COPILOT: " + await engine.GetResponse("sit in copilot seat"));
        TestContext.Out.WriteLine("EXAMINE COPILOT: " + await engine.GetResponse("examine copilot seat"));
    }

    [Test]
    public async Task Probe_StaleSubLocation_SurvivesLiftoffWithoutSitting()
    {
        var engine = GetTarget();
        await WalkToTruckWithFloyd(engine);
        await engine.GetResponse("sit in pilot seat");
        await engine.GetResponse("put activation in slot");
        Repository.GetItem<Chronometer>().CurrentTime = PinnedTime;
        await engine.GetResponse($"type {ExpectedCourse}");

        // Walk out of the truck while still "seated", then come back and close the hatch.
        TestContext.Out.WriteLine("OUT: " + await engine.GetResponse("out"));
        TestContext.Out.WriteLine("SubLocation after walking out: " +
                                  (Repository.GetLocation<Spacetruck>().SubLocation?.GetType().Name ?? "null"));
        TestContext.Out.WriteLine("IN: " + await engine.GetResponse("in"));
        TestContext.Out.WriteLine("CLOSE: " + await engine.GetResponse("close hatch"));
        var log = string.Empty;
        for (var i = 0; i < 8; i++)
            log += await engine.GetResponse("wait");
        TestContext.Out.WriteLine("LOG: " + log);
    }
}
