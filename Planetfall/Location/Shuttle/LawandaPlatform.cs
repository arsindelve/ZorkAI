using GameEngine.Location;
using Planetfall.Location.Lawanda;

namespace Planetfall.Location.Shuttle;

/// <summary>
/// <remarks>
/// Both trains can never be at the other station. If you teleport back and ride the other train, the elevator will be stuck at the top,
/// and the platform has no elevator call button. 
/// </remarks>
/// </summary>
internal class LawandaPlatform : LocationWithNoStartingItems
{
    public override string Name => "Lawanda Platform";

    private bool AlifeIsHere => Repository.GetLocation<AlfieControlWest>().TunnelPosition == 0;

    private bool BettyIsHere => Repository.GetLocation<BettyControlWest>().TunnelPosition == 0;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<Escalator>() },
            {
                Direction.S,
                new MovementParameters
                {
                    CanGo = _ => AlifeIsHere, Location = Repository.GetLocation<ShuttleCarAlfie>(),
                    CustomFailureMessage = "You can't go that way. "
                }
            },
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => BettyIsHere, Location = Repository.GetLocation<ShuttleCarBetty>(),
                    CustomFailureMessage = "You can't go that way. "
                }
            }
        };
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(4);
    }
    
    protected override string GetContextBasedDescription(IContext context)
    {
        // I do not think it is possible, in the game, for both shuttles to be gone from here 
        
        return
            "This is a wide, flat strip of concrete. " +
            (AlifeIsHere && BettyIsHere ? "Open shuttle cars lie to the north and south. " : "" ) +
            (AlifeIsHere && !BettyIsHere ? "An open shuttle car lies to the south. " : "") +
            (!AlifeIsHere && BettyIsHere ? "An open shuttle car lies to the north. " : "") +
            "A wide escalator, not currently operating, beckons upward at the east end of the platform. A faded sign " +
            "reads \"Shutul Platform -- Lawanda Staashun.\"";
    }
}