using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Shuttle;

/// <summary>
/// <remarks>
/// If Alfie and Betty are here at the same time, you cannot go up the elevator. If you teleport back and ride the other train, the elevator will be stuck at the top,
/// and the platform has no elevator call button. 
/// </remarks>
/// </summary>
internal class KalamonteePlatform : LocationWithNoStartingItems
{
    public override string Name => "Kalamontee Platform";

    private bool AlifeIsHere => Repository.GetLocation<AlfieControlEast>().TunnelPosition == 0;

    private bool BettyIsHere => Repository.GetLocation<BettyControlEast>().TunnelPosition == 0;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<WaitingArea>() },
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

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a wide, flat strip of concrete which continues westward. " +
            (AlifeIsHere && BettyIsHere ? "Open shuttle cars lie on the north and south sides of the platform. " : "") +
            (BettyIsHere && !AlifeIsHere ? "An open shuttle car lies to the north." : "") +
            (AlifeIsHere && !BettyIsHere
                ? "A large transport of some sort lies to the south, its open door beckoning you to enter. "
                : "") +
            "A faded sign on the wall reads \"Shutul Platform -- Kalamontee Staashun.\"";
    }
}