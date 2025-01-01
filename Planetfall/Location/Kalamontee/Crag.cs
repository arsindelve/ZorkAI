using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

// TODO: Sleep here and die. 

public class Crag : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.Up, Go<Balcony>() },
            { Direction.Down, Go<Underwater>() }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "You have reached a cleft in the cliff wall where the island rises from the water. " +
        "The edge of the cleft displays recently exposed rock where it collapsed under the weight of " +
        "the escape pod. About two meters below, turbulent waters swirl against sharp rocks. A small " +
        "structure clings to the face of the cliff about eight meters above you. Even an out-of-shape " +
        "Ensign Seventh Class could probably climb up to it. ";

    public override string Name => "Crag";

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(3);
        base.OnFirstTimeEnterLocation(context);
    }

    public string DeathDescription =>
        "Suddenly, in the middle of the night, a wave of water washes over you. Before you can quite get your bearings, you drown. ";
}