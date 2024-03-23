namespace ZorkOne.Location;

public class EndOfRainbow : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.SW, new MovementParameters { Location = GetLocation<CanyonBottom>() }
        }
    };


    protected override string ContextBasedDescription =>
        "You are on a small, rocky beach on the continuation of the Frigid River past the Falls. The beach is narrow " +
        "due to the presence of the White Cliffs. The river canyon opens here and sunlight shines in from above. " +
        "A rainbow crosses over the falls to the east and a narrow path continues to the southwest.";

    public override string Name => "End of Rainbow";

    public override InteractionResult RespondToSpecificLocationInteraction(string input, IContext context)
    {
        switch (input.ToLowerInvariant().Trim())
        {
            case "wave sceptre":
            case "wave the sceptre":
                throw new NotImplementedException();
        }

        return base.RespondToSpecificLocationInteraction(input, context);
    }
}