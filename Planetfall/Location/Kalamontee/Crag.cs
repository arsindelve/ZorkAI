using GameEngine.Location;

namespace Planetfall.Location.Kalamontee;

// TODO: Sleep here and die. 

public class Crag : LocationWithNoStartingItems
{
    public override string Name => "Crag";

    public string DeathDescription =>
        "Suddenly, in the middle of the night, a wave of water washes over you. Before you can quite get your bearings, you drown. ";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Up, Go<Balcony>() },
            { Direction.Down, Go<Underwater>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You have reached a cleft in the cliff wall where the island rises from the water. " +
               "The edge of the cleft displays recently exposed rock where it collapsed under the weight of " +
               "the escape pod. About two meters below, turbulent waters swirl against sharp rocks. A small " +
               "structure clings to the face of the cliff about eight meters above you. Even an out-of-shape " +
               "Ensign Seventh Class could probably climb up to it. ";
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.SystemPromptAddendum =
            "The Feinstein exploded from an unknown accident, and the player, having survived in an escape pod, is " +
            "now stranded in what seems to be an abandoned complex on an unknown planet. ";
        
        context.AddPoints(3);
        base.OnFirstTimeEnterLocation(context);
    }
}