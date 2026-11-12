using GameEngine;
using UnitTests.Parser.Census;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Parser;

/// <summary>How to start a hermetic copy of Zork I for one census probe.</summary>
public static class ZorkOneCensus
{
    public static CensusGame Game { get; } = CensusGame.For<ZorkI, ZorkIContext>(
        boot: _ => Repository.GetLocation<WestOfHouse>().Init(),
        light: context =>
        {
            var lantern = Repository.GetItem<Lantern>();
            context.Take(lantern);
            lantern.IsOn = true;
        });
}
