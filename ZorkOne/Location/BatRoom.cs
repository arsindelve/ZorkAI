namespace ZorkOne.Location;

internal class BatRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.S, new MovementParameters { Location = GetLocation<SqueakyRoom>() }
            }
        };

    protected override string ContextBasedDescription =>
        "You are in a small room which has doors only to the east and south. ";

    public override string Name => "Bat Room";

    public override string BeforeEnterLocation(IContext context)
    {
        context.CurrentLocation = GetLocation<MineEntrance>();
        var batText = """
                      A large vampire bat, hanging from the ceiling, swoops down at you!
                       
                          Fweep!
                          Fweep!
                          Fweep!
                       
                      The bat grabs you by the scruff of your neck and lifts you away....
                      """;

        return $"{batText}\n\n{context.CurrentLocation.Description} ";
    }

    public override void Init()
    {
        StartWithItem<Bat>(this);
    }
}