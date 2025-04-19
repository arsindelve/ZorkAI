using Planetfall.Item.Kalamontee;

namespace Planetfall.Item.Lawanda.Lab;

internal class PieceOfPaper : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["piece of paper", "paper"];

    public string ExaminationDescription => ReadDescription;

    public string ReadDescription =>
        $"Week uv 14-Juun--2882. Kombinaashun tuu Konfurins Ruum: {Repository.GetItem<ConferenceRoomDoor>().UnlockCode}.";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a piece of paper here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A piece of paper";
    }
}