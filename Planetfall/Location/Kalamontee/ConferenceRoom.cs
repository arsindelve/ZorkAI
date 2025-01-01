using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

internal class ConferenceRoom : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        throw new NotImplementedException();
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a fairly large room, almost filled by a round conference table. To the south is a door which is TODO. To the north is a small room about the size of a phone booth. ";
    }

    public override string Name => "Conference Room";
    public override void Init()
    {
       
    }
}