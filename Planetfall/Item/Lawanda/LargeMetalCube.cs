using GameEngine.Item;
using GameEngine.Location;
using Model.Location;

namespace Planetfall.Item.Lawanda;

public class LargeMetalCube : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["large metal cube", "metal cube", "cube", "lid"];

    public string ExaminationDescription => $"The large metal cube is {(IsOpen ? "open" : "closed")}. ";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "In one corner is a large metal cube whose lid is open.";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A large metal cube";
    }
    
    public override void Init()
    {
        IsOpen = true;
    }
}
