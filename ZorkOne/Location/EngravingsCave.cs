namespace ZorkOne.Location;

public class EngravingsCave : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.NW, new MovementParameters { Location = GetLocation<RoundRoom>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<DomeRoom>() } },
        };

    public EngravingsCave()
    {
        StartWithItem(Repository.GetItem<Engravings>(), this);
    }
    
    public override string Name => "Engravings Cave";

    protected override string ContextBasedDescription =>
        "You have entered a low cave with passages leading northwest and east.\n\nThere are old engravings on the walls here.";
}