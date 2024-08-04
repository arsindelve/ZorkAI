using GameEngine.Item;

namespace ZorkOne.Item;

public class Engravings : ItemBase, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["engravings"];

    string ICanBeExamined.ExaminationDescription => ((ICanBeRead)this).ReadDescription;

    string ICanBeRead.ReadDescription =>
        "The engravings were incised in the living rock of the cave wall by an unknown hand. They depict, in symbolic form, " +
        "the beliefs of the ancient peoples of Zork. Skillfully interwoven with the bas reliefs are excerpts illustrating the " +
        "major tenets expounded by the sacred texts of the religion of that time. Unfortunately, a later age seems to have considered " +
        "them blasphemous and just as skillfully excised them.";
}