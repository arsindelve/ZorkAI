using GameEngine.Item;
using Model.Interface;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Item;

public class Grating : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForMatching => ["grating"];

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return currentLocation switch
        {
            Clearing => "There is a grating securely fastened into the ground. ",
            GratingRoom => "Above you is a grating locked with a skull-and-crossbones lock. ",
            _ => throw new NotSupportedException()
        };
    }

    public override string GenericDescription(ILocation currentLocation) => NeverPickedUpDescription(currentLocation);

    public string ExaminationDescription => $"The grating is {(IsOpen ? "open" : "closed")}. ";

    public bool IsOpen { get; set; }

    public bool IsLocked { get; set; } = true;

    public string NowOpen => "";

    public string NowClosed => "";

    public string AlreadyOpen => "";

    public string AlreadyClosed => "";

    public bool HasEverBeenOpened { get; set; }

    public string? CannotBeOpenedDescription(IContext context)
    {
        return IsLocked ? "The grating is locked. " : null;
    }
}