using GameEngine;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Feinstein;
using UnitTests.Parser.Census;

namespace Planetfall.Tests.Parser;

/// <summary>How to start a hermetic copy of Planetfall for one census probe.</summary>
public static class PlanetfallCensus
{
    public static CensusGame Game { get; } = CensusGame.For<PlanetfallGame, PlanetfallContext>(
        boot: _ => Repository.GetLocation<DeckNine>().Init(),
        light: context =>
        {
            var lamp = Repository.GetItem<Lamp>();
            context.Take(lamp);
            lamp.IsOn = true;
        });
}
