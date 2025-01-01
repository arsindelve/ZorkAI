using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class Studio : LocationBase
{
    public override string Name => "Studio";

    protected override string GetContextBasedDescription(IContext context) =>
        "This appears to have been an artist's studio. The walls and floors are splattered with paints of 69 different colors. " +
        "Strangely enough, nothing of value is hanging here. At the south end of the room is an open door (also covered with paint). " +
        "A dark and narrow chimney leads up from a fireplace; although you might be able to get up it, it seems unlikely you could get back down. ";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
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

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context, IGenerationClient client)
    {
        switch (input?.ToLowerInvariant().Trim())
        {
            case "climb":
            case "climb up":
            case "climb up chimney":
            case "climb chimney":

                context.CurrentLocation = Repository.GetLocation<Kitchen>();
                var message = Repository.GetLocation<Kitchen>().GetDescription(context);
                return new PositiveInteractionResult(message);
        }

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }

    public override void Init()
    {
        StartWithItem<Manual>();
    }
}