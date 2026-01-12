using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee.Dorm;

internal class DormD : LocationWithNoStartingItems
{
    public override string Name => "Dorm D";

    public override void Init()
    {
        StartWithItem<Bed>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<DormCorridor>() },
            { Direction.N, Go<SanfacD>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a very long room lined with multi-tiered bunks. Flimsy partitions between the tiers may have " +
               "provided a modicum of privacy. These spartan living quarters could have once housed many hundreds, but it " +
               "seems quite deserted now. There are openings at the north and south ends of the room.";
    }

    public override Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        switch (input?.ToLowerInvariant().Trim())
        {
            case "enter bed":
            case "get in bed":
            case "climb in bed":
            case "get in bunk":
            case "climb in bunk":
            case "lie down":
            case "lie down in bed":
            case "lay down":
            case "lay down in bed":
                var bed = Repository.GetItem<Bed>();
                var bedLocation = Repository.GetLocation<BedLocation>();
                bedLocation.ParentLocation = this;
                context.CurrentLocation = bedLocation;
                var message = bed.GetIn(context);
                return Task.FromResult<InteractionResult>(new PositiveInteractionResult(message));
        }

        return base.RespondToSpecificLocationInteraction(input, context, client);
    }
}