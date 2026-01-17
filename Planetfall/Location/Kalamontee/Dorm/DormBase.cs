using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee.Dorm;

/// <summary>
/// Base class for all dormitory rooms. Provides shared functionality including
/// bed initialization, room description, and bed-related command handling.
/// </summary>
internal abstract class DormBase : LocationWithNoStartingItems
{
    public override void Init()
    {
        StartWithItem<Bed>();
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
        if (BedCommands.IsBedEntryCommand(input))
        {
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
