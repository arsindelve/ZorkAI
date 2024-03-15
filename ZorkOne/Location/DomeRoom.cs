namespace ZorkOne.Location;

public class DomeRoom : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<EngravingsCave>() } },
            {
                Direction.Down,
                new MovementParameters
                    { CanGo = _ => false, CustomFailureMessage = "You cannot go down without breaking many bones." }
            }
        };

    protected override string Name => "Dome Room";

    // TODO: Implement fatal jump. 
    
    protected override string ContextBasedDescription =>
        "You are at the periphery of a large dome, which forms the ceiling of another room below. " +
        "Protecting you from a precipitous drop is a wooden railing which circles the dome.";
    
}