using Model.Movement;

namespace ZorkOne.Location;

internal class MachineRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.N, new MovementParameters { Location = GetLocation<DraftyRoom>() }
        }
    };

    protected override string ContextBasedDescription =>
        "This is a large, cold room whose sole exit is to the north. In one corner there is a machine which is " +
        "reminiscent of a clothes dryer. On its face is a switch which is labelled \"START\". The switch does not " +
        "appear to be manipulable by any human hand (unless the fingers are about 1/16 by 1/4 inch). On the front " +
        $"of the machine is a large lid, which is {(Repository.GetItem<Machine>().IsOpen ? "open. \n" + Repository.GetItem<Machine>().ItemListDescription("machine") : "closed. ")} ";

    public override string Name => "Machine Room";

    public override void Init()
    {
        StartWithItem<Machine>(this);
    }
}