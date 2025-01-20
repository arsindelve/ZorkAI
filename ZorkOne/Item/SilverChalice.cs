using GameEngine;
using GameEngine.Item;

namespace ZorkOne.Item;

public class SilverChalice : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenFirstPickedUp,
    IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching =>
        ["silver", "chalice", "silver chalice"];

    public override string? CannotBeTakenDescription =>
        Repository.GetItem<Thief>().IsDead || Repository.GetItem<Thief>().IsUnconscious
            ? ""
            : "Realizing just in time that you'd be stabbed in the back if you attempted to take the chalice, you return to the fray. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a silver chalice here";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A chalice";
    }
}