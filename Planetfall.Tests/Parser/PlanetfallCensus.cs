using GameEngine;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Feinstein;
using UnitTests.Parser.Census;

namespace Planetfall.Tests.Parser;

/// <summary>
///     How to start a hermetic copy of Planetfall for one census probe - and the same copy every time.
///     Planetfall rolls dice as a game begins: the time of day on the watch, from which the hunger and
///     sleep clocks are set, and the charge in each battery. Left alone, no two fresh games are in the
///     same state, and every turn in one looks like an effect beside any turn in another.
/// </summary>
public static class PlanetfallCensus
{
    public static CensusGame Game { get; } = CensusGame.For<PlanetfallGame, PlanetfallContext>(
        boot: context =>
        {
            Repository.GetLocation<DeckNine>().Init();

            // The watch first, then the game's own start-up, which the test bases skip: it dresses the
            // player and sets the hunger and sleep clocks from the time on the watch. Without it the
            // sleep clock stands at zero and every turn begins "You begin to feel weary".
            Repository.GetItem<Chronometer>().CurrentTime = StartOfTheGame;
            context.Init();
        },
        light: context =>
        {
            var lamp = Repository.GetItem<Lamp>();
            context.Take(lamp);
            lamp.IsOn = true;
        },
        settle: _ =>
        {
            // Rolled as the objects are made, before any dice can be pinned.
            Repository.GetItem<FreshBattery>().ChargesRemaining = 25;
            Repository.GetItem<OldBattery>().ChargesRemaining = 4;

            // The one chooser that belongs to no object.
            SleepEngine.Chooser = InteractionCensus.PinnedDice;
        },
        afterRestart: restarted =>
        {
            // A death starts the game over with a new watch, set at random again, and clocks set from it.
            var context = (PlanetfallContext)restarted;
            Repository.GetItem<Chronometer>().CurrentTime = StartOfTheGame;
            context.HungerNotifications.Initialize(StartOfTheGame);
            context.SleepNotifications.Initialize(StartOfTheGame);
        });

    private const int StartOfTheGame = 4600;
}
