using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

public class Crag : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Up, Go<Balcony>() },
            { Direction.Down, Go<Underwater>() }
        };

    protected override string ContextBasedDescription =>
        "You have reached a cleft in the cliff wall where the island rises from the water. " +
        "The edge of the cleft displays recently exposed rock where it collapsed under the weight of " +
        "the escape pod. About two meters below, turbulent waters swirl against sharp rocks. A small " +
        "structure clings to the face of the cliff about eight meters above you. Even an out-of-shape " +
        "Ensign Seventh Class could probably climb up to it. ";

    public override string Name => "Crag";

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.SystemPromptAddendum =
            "The Feinstein exploded from an unknown accident, and the player, having survived in an escape pod, is " +
            "now stranded in what seems to be an abandoned complex on an unknown planet. ";
        
        context.AddPoints(3);
        base.OnFirstTimeEnterLocation(context);
    }
}