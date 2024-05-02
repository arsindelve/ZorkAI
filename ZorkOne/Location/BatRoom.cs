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
        if (!context.HasItem<Garlic>())
        {
            // TODO: Come from the east without the garlic, he's dropping you in the mine! 
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

        return base.BeforeEnterLocation(context);
    }

    public override string AfterEnterLocation(IContext context)
    {
        // TODO: Sword glows. 
        
        if (context.HasItem<Garlic>())
            return
                "\nIn the corner of the room on the ceiling is a large vampire bat who is obviously deranged and holding his nose. ";

        return base.AfterEnterLocation(context);
    }

    public override void Init()
    {
        StartWithItem<Bat>(this);
        StartWithItem<JadeFigurine>(this);
    }
}