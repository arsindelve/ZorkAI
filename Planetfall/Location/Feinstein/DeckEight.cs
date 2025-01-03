﻿using Model.Movement;

namespace Planetfall.Location.Feinstein;

internal class DeckEight : BlatherLocation
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.Down, Go<Gangway>() },
            {
                Direction.W,
                BlatherBlocksYou()
            },
            {
                Direction.N,
                BlatherBlocksYou()
            },
            {
                Direction.E,
                BlatherBlocksYou()
            }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This is a featureless corridor leading port and starboard. A gangway leads down, and to fore " +
        "is the Hyperspatial Jump Machinery Room. ";

    public override string Name => "Deck Eight";
}