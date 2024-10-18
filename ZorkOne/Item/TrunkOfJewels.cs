using GameEngine.Item;

namespace ZorkOne.Item;

public class TrunkOfJewels : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp, ICanBeExamined
{
    public override string[] NounsForMatching => ["trunk", "jewels", "trunk of jewels"];

    public override string GenericDescription(ILocation? currentLocation) => "A trunk of jewels";

    public string OnTheGroundDescription(ILocation currentLocation) => "There is a trunk of jewels here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) =>
        "Lying half buried in the mud is an old trunk, bulging with jewels. ";

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 15;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;

    public override int Size => 7;
    
    public string ExaminationDescription => "There are lots of jewels in there. ";
}