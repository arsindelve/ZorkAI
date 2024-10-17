using GameEngine.Item;

namespace ZorkOne.Item;

public class Label : ItemBase, ICanBeExamined, ICanBeRead, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["label", "tan label"];
    public string OnTheGroundDescription(ILocation currentLocation) => "There is a tan label here. ";

    public override string GenericDescription(ILocation? currentLocation) => "A tan label";
    public string ExaminationDescription => ReadDescription;

    public string ReadDescription => """
                                     !!!!FROBOZZ MAGIC BOAT COMPANY!!!!
                                      
                                     Hello, Sailor!
                                      
                                     Instructions for use:
                                      
                                        To get into a body of water, say "Launch".
                                        To get to shore, say "Land" or the direction in which you want to maneuver the boat.
                                      
                                     Warranty:
                                      
                                       This boat is guaranteed against all defects for a period of 76 milliseconds from date of purchase or until first used, whichever comes first.
                                      
                                     Warning:
                                        This boat is made of thin plastic.
                                        Good Luck!
                                     """;
}