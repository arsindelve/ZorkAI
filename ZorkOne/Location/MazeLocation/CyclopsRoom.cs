using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class CyclopsRoom : DarkLocationWithNoStartingItems{
    
    protected override Dictionary<Direction, MovementParameters> Map { get; }

    protected override string ContextBasedDescription =>
        "This room has an exit on the northwest, and a staircase leading up.\n\nA cyclops, who looks prepared to eat " +
        "horses (much less mere adventurers), blocks the staircase. From his state of health, and the bloodstains " +
        "on the walls, you gather that he is not very friendly, though he likes people.";
    
    public override string Name => "Cyclops Room";
}