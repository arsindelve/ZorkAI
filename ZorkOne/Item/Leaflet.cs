using GameEngine.Item;

namespace ZorkOne.Item;

public class Leaflet : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["leaflet"];

    public override string InInventoryDescription => "A leaflet";

    string ICanBeExamined.ExaminationDescription => """
                                                    "WELCOME TO ZORK!

                                                    ZORK is a game of adventure, danger, and low cunning. In it you will explore some of the most amazing territory ever seen by mortals. No computer should be without one!"
                                                    """;

    string ICanBeRead.ReadDescription => ((ICanBeExamined)this).ExaminationDescription;

    string ICanBeTakenAndDropped.OnTheGroundDescription => "A small leaflet is on the ground. ";
}