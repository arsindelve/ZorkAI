using GameEngine.Item;

namespace ZorkOne.Item;

public class Guidebook : ItemBase, ICanBeTakenAndDropped, ICanBeRead, ICanBeExamined, IPluralNoun
{
    public override string[] NounsForMatching =>
        ["book", "books", "guidebook", "guidebooks", "guide book", "guide books"];

    public override string GenericDescription(ILocation? currentLocation) => "A tour guidebook ";

    string ICanBeExamined.ExaminationDescription => ReadDescription;

    public string ReadDescription =>
        """

            "Flood Control Dam #3
             
            FCD#3 was constructed in year 783 of the Great Underground Empire to harness the mighty Frigid River. This work was supported by a grant of 37 million zorkmids from your omnipotent local tyrant Lord Dimwit Flathead the Excessive. This impressive structure is composed of 370,000 cubic feet of concrete, is 256 feet tall at the center, and 193 feet wide at the top. The lake created behind the dam has a volume of 1.7 billion cubic feet, an area of 12 million square feet, and a shore line of 36 thousand feet.
             
            The construction of FCD#3 took 112 days from ground breaking to the dedication. It required a work force of 384 slaves, 34 slave drivers, 12 engineers, 2 turtle doves, and a partridge in a pear tree. The work was managed by a command team composed of 2345 bureaucrats, 2347 secretaries (at least two of whom could type), 12,256 paper shufflers, 52,469 rubber stampers, 245,193 red tape processors, and nearly one million dead trees.
             
            We will now point out some of the more interesting features of FCD#3 as we conduct you on a guided tour of the facilities:
             
                    1) You start your tour here in the Dam Lobby. You will notice on your right that....

            """;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a tour guidebook here.";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Some guidebooks entitled \"Flood Control Dam #3\" are on the reception desk.";
    }
}
