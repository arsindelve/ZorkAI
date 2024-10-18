using GameEngine;
using GameEngine.Item;

namespace ZorkOne.Item;

public class Nest : ContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["nest", "bird nest", "bird's nest"];

    public override bool IsTransparent => true;

    public override string GenericDescription(ILocation? currentLocation) =>
        "A bird's nest " + (Items.Any() ? $"\n{ItemListDescription("bird's nest", currentLocation)}" : "");

    public override int Size => 1;

    public string ExaminationDescription => HasItem<Egg>()
        ? "In the bird's nest is a large egg encrusted with precious jewels, " +
          "apparently scavenged by a childless songbird. The egg is covered with " +
          "fine gold inlay, and ornamented in lapis lazuli and mother-of-pearl. " +
          "Unlike most eggs, this one is hinged and closed with a delicate " +
          "looking clasp. The egg appears extremely fragile."
        : "The bird's nest is empty. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a bird's nest here. " +
               (Items.Any()
                   ? $"\n{ItemListDescription("bird's nest", currentLocation)}"
                   : "");
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "\n Beside you on the branch is a small bird's nest. " +
               (HasItem<Egg>() && !Repository.GetItem<Egg>().HasEverBeenPickedUp
                   ? "\n" + ExaminationDescription
                   : Items.Any()
                       ? $"\n{ItemListDescription("bird's nest", currentLocation)}"
                       : "");
    }

    public override void Init()
    {
        StartWithItemInside<Egg>();
    }

    public override string ItemListDescription(string name, ILocation? location)
    {
        if (Items.Count == 1 && HasItem<Egg>() && !Repository.GetItem<Egg>().HasEverBeenPickedUp)
            return ExaminationDescription;

        return base.ItemListDescription(name, location);
    }
}