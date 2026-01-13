using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

/// <summary>
/// Represents the player being in a bed.
/// This is a sub-location that shares the same physical space as the dormitory room.
/// </summary>
internal class BedLocation : LocationWithNoStartingItems, ISubLocation
{
    public override string Name => "In Bed";

    public ILocation ParentLocation { get; set; } = null!;

    public string LocationDescription => "You are lying in one of the bunk beds. ";

    public string GetIn(IContext context)
    {
        var bed = Repository.GetItem<Bed>();
        return bed.GetIn(context);
    }

    public string GetOut(IContext context)
    {
        var bed = Repository.GetItem<Bed>();
        var result = bed.GetOut(context);

        // Only move player back to parent location if they were actually allowed to leave
        // (Bed.GetOut returns different messages based on whether exit was allowed)
        if (!bed.PlayerInBed) context.CurrentLocation = ParentLocation;

        return result;
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // No movement while in bed - player must exit first
        return new Dictionary<Direction, MovementParameters>();
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return LocationDescription;
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        // Set the parent location to wherever the player was before getting in bed
        if (previousLocation is not ISubLocation) ParentLocation = previousLocation;

        return base.BeforeEnterLocation(context, previousLocation);
    }

    public override Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        if (BedCommands.IsBedExitCommand(input))
            return Task.FromResult<InteractionResult>(new PositiveInteractionResult(GetOut(context)));

        return base.RespondToSpecificLocationInteraction(input, context, client);
    }
}