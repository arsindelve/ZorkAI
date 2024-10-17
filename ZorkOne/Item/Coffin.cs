using GameEngine;
using GameEngine.Item;

namespace ZorkOne.Item;

public class Coffin : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped,
    IGivePointsWhenPlacedInTrophyCase, IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["coffin", "solid-gold coffin", "solid gold coffin", "gold coffin"];

    public override string NowOpen(ILocation currentLocation) => "The gold coffin opens. " + (Items.Contains(Repository.GetItem<Sceptre>())
        ? "A sceptre, possibly that of ancient Egypt itself, is in the coffin. The sceptre is ornamented with colored enamel, and tapers to a sharp point."
        : "");

    public override int Size => 15;

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen
            ? Items.Count == 1 && Items.Single() == Repository.GetItem<Sceptre>()
                ? "A sceptre, possibly that of ancient Egypt itself, is in the coffin. The sceptre is ornamented with colored enamel, and tapers to a sharp point."
                : ItemListDescription("gold coffin", null)
            : "The gold coffin is closed. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "The solid-gold coffin used for the burial of Ramses II is here. " +
               (IsOpen & Items.Any()
                   ? $"\n{ItemListDescription("gold coffin", null)}"
                   : "");
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 15;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return !IsOpen
            ? "A gold coffin."
            : "A gold coffin " + (Items.Any() ? $"\n{ItemListDescription("gold coffin", null)}" : "");
    }

    public override void Init()
    {
        StartWithItemInside<Sceptre>();
    }
}