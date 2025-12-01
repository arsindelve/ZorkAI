using GameEngine.Location;
using Model.Location;

namespace Planetfall.Item.Lawanda;

public class MedicalRobotBreastPlate : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["breastplate", "medical robot breastplate", "medical breastplate", "robot breastplate", "plate"];

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "There is a medical robot breastplate here. ";
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a medical robot breastplate here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "It is a dented breastplate from a medical robot. ";
    }

    public override string Name => "medical robot breastplate";
}
