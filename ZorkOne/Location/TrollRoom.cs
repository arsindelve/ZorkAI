namespace ZorkOne.Location;

public class TrollRoom : DarkLocation
{
     private bool TrollIsAlive => Repository.GetItem<Troll>().CurrentLocation == Repository.GetLocation<TrollRoom>();

    public TrollRoom()
    {
        StartWithItem(Repository.GetItem<Troll>(), this);
    }

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<Cellar>() } },
            {
                // Direction.E,
                // new MovementParameters
                // {
                //     Location = GetLocation<EastWestPassage>(), CanGo = _ => !TrollIsAlive,
                //     CustomFailureMessage = "The troll fends you off with a menacing gesture."
                // }
                Direction.E, new MovementParameters{ Location = GetLocation<EastWestPassage>()}
            }
        };

    public override string Name => "The Troll Room";

    protected override string ContextBasedDescription =>
        "This is a small room with passages to the east and south and a forbidding hole leading west. " +
        "Bloodstains and deep scratches (perhaps made by an axe) mar the walls. ";

    public override string AfterEnterLocation(IContext context)
    {
        var swordInPossession = context.HasItem<Sword>();

        if (TrollIsAlive && swordInPossession)
            return "Your sword has begun to glow very brightly.";

        return string.Empty;
    }
}