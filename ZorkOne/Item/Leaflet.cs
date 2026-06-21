using GameEngine.Item;

namespace ZorkOne.Item;

public class Leaflet : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    // The original Zork I source declares the leaflet (ADVERTISEMENT object) with the synonyms
    // ADVERTISEMENT, LEAFLET, MAIL, BOOKLET, and PAMPHLET, so the player can refer to the
    // mailbox's contents as "mail", "booklet", etc. The port only had "leaflet", so natural
    // early-game commands like "read mail" failed to resolve to the item.
    public override string[] NounsForMatching =>
        ["leaflet", "mail", "booklet", "pamphlet", "advertisement"];

    public override bool IsSoft => true;

    string ICanBeExamined.ExaminationDescription => """
                                                    "WELCOME TO ZORK!

                                                    ZORK is a game of adventure, danger, and low cunning. In it you will explore some of the most amazing territory ever seen by mortals. No computer should be without one!"
                                                    """;

    string ICanBeRead.ReadDescription => ((ICanBeExamined)this).ExaminationDescription;

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "A small leaflet is on the ground. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A leaflet";
    }
}