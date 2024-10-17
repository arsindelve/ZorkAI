using Model.Movement;

namespace Planetfall.Location.Feinstein;

internal class DeckEight : BlatherLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Down, Go<Gangway>() },
            {
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "Blather throws you to the deck and makes you do 20 push-ups. "
                }
            },
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "Blather throws you to the deck and makes you do 20 push-ups. "
                }
            },
            {
                Direction.E,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "Blather throws you to the deck and makes you do 20 push-ups. "
                }
            }
        };

    protected override string ContextBasedDescription =>
        "This is a featureless corridor leading port and starboard. A gangway leads down, and to fore " +
        "is the Hyperspatial Jump Machinery Room. ";

    public override string Name => "Deck Eight";
}