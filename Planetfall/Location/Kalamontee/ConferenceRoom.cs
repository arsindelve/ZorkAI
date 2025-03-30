using GameEngine.Location;

namespace Planetfall.Location.Kalamontee;

internal class ConferenceRoom : LocationBase
{
    public override string Name => "Conference Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<BoothOne>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        // TODO: Locked door. 
        return
            "This is a fairly large room, almost filled by a round conference table. To the south is a door which is TODO. To the north is a small room about the size of a phone booth. ";
    }

    public override void Init()
    {
    }
}