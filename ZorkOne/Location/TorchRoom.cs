namespace ZorkOne.Location;

public class TorchRoom : DarkLocation
{
    public TorchRoom()
    {
        StartWithItem(GetItem<Torch>(), this);
    }
    
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<Temple>() } },
            {
                Direction.Up,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "You cannot reach the rope."
                }
            }
        };
    
    public override string Name => "Torch Room";

    protected override string ContextBasedDescription
    {
        get
        {
            var desc =
                "This is a large room with a prominent doorway leading to a down staircase. To the west is a narrow twisting tunnel, " +
                "covered with a thin layer of dust.  Above you is a large dome painted with scenes depicting elfin hacking rites. " +
                 "Up around the edge of the dome (20 feet up) is a wooden railing. In the center of the room there is a white marble pedestal. ";

            if (GetItem<Rope>().TiedToRailing)
                desc += "A piece of rope descends from the railing above, ending some five feet above your head. ";

            return desc + GetItemDescriptions();
        }
    }
}