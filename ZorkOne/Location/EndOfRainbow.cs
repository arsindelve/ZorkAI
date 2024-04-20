namespace ZorkOne.Location;

public class EndOfRainbow : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.SW, new MovementParameters { Location = GetLocation<CanyonBottom>() }
        },
        {
            Direction.E, new MovementParameters { Location = GetLocation<OnTheRainbow>(), CanGo = _ => RainbowIsSolid }
        }
    };

    public bool RainbowIsSolid { get; set; }

    protected override string ContextBasedDescription =>
        "You are on a small, rocky beach on the continuation of the Frigid River past the Falls. The beach is narrow " +
        "due to the presence of the White Cliffs. The river canyon opens here and sunlight shines in from above. " +
        "A rainbow crosses over the falls to the east and a narrow path continues to the southwest. ";

    public override string Name => "End of Rainbow";

    public override InteractionResult RespondToSpecificLocationInteraction(string? input, IContext context)
    {
        if (!context.HasItem<Sceptre>() && GetItem<Sceptre>().CurrentLocation == GetLocation<EndOfRainbow>())
            return new PositiveInteractionResult("You don't have the sceptre. ");

        if (!context.HasItem<Sceptre>())
            return new NoNounMatchInteractionResult();

        switch (input?.ToLowerInvariant().Trim())
        {
            case "wave sceptre":
            case "wave the sceptre":
                return WaveTheSceptre();
        }

        return base.RespondToSpecificLocationInteraction(input, context);
    }

    private InteractionResult WaveTheSceptre()
    {
        if (RainbowIsSolid)
        {
            RainbowIsSolid = false;
            return new PositiveInteractionResult("The rainbow seems to have become somewhat run-of-the-mill.");
        }

        RainbowIsSolid = true;

        var oldLocation = Repository.GetItem<PotOfGold>().CurrentLocation;
        
        if (oldLocation == null)
            ItemPlacedHere(GetItem<PotOfGold>());

        return new PositiveInteractionResult(
            "Suddenly, the rainbow appears to become solid and, I venture, walkable (I think " +
            "the giveaway was the stairs and bannister). " + (oldLocation != null 
                ? ""
                : " A shimmering pot of gold appears at the end of the rainbow. \n"));
    }
}