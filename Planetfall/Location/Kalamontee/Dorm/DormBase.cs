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
    // Shared destination-navigation synonyms for the dormitories (issue #268 review: de-duplicated
    // from three byte-identical arrays on Dorm B/C/D). Dorm A overrides with its own list — it has a
    // "bunkroom" alias and lacks "bedroom"/"quarters".
    public override string[] NounsForMatching => ["dormitory", "bedroom", "barracks", "quarters"];

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

    // Shared by all four dormitories (they inherit this base's prose). The partitions are scenery.
    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["partition", "partitions", "flimsy partition", "flimsy partitions"],
            "The flimsy partitions are plain dividers, put up to break the long room into smaller spaces. ",
            "The partitions are fixed between the bunks. ")
    ];

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
