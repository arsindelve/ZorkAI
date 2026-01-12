using GameEngine.Location;
using Planetfall.Item.Kalamontee;
using Planetfall.Location.Kalamontee.Dorm;

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

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // No movement while in bed - player must exit first
        return new Dictionary<Direction, MovementParameters>();
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return LocationDescription;
    }

    public string? BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        // Set the parent location to wherever the player was before getting in bed
        if (previousLocation is not ISubLocation)
        {
            ParentLocation = previousLocation;
        }

        return null;
    }

    public string GetIn(IContext context)
    {
        var bed = Repository.GetItem<Bed>();
        return bed.GetIn(context);
    }

    public string GetOut(IContext context)
    {
        var bed = Repository.GetItem<Bed>();
        var result = bed.GetOut(context);

        // Move player back to parent location
        context.CurrentLocation = ParentLocation;

        return result;
    }
}
