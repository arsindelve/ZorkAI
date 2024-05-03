namespace ZorkOne.Location;

public class Studio : BaseLocation
{
    public override string Name => "Studio";

    protected override string ContextBasedDescription =>
        "This appears to have been an artist's studio. The walls and floors are splattered with paints of 69 different colors. " +
        "Strangely enough, nothing of value is hanging here. At the south end of the room is an open door (also covered with paint). " +
        "A dark and narrow chimney leads up from a fireplace; although you might be able to get up it, it seems unlikely you could get back down. ";

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<Gallery>() } },
            {
                Direction.Up,
                new MovementParameters
                {
                    Location = GetLocation<Kitchen>(), 
                    WeightLimit = 7,
                    WeightLimitFailureMessage = "You can't get up there with what you're carrying. "
                }
            }
        };

    public override InteractionResult RespondToSpecificLocationInteraction(string? input, IContext context)
    {
        switch (input?.ToLowerInvariant().Trim())
        {
            case "climb":
            case "climb up":
            case "climb up chimney":
            case "climb chimney":

                context.CurrentLocation = Repository.GetLocation<Kitchen>();
                var message = Repository.GetLocation<Kitchen>().Description;
                return new PositiveInteractionResult(message);
        }

        return base.RespondToSpecificLocationInteraction(input, context);
    }

    public override void Init()
    {
        StartWithItem<Manual>(this);
    }
}