using GameEngine.Item;

namespace EscapeRoom.Item;

public class WallSign : ItemBase, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["sign", "wall sign"];

    public override string? CannotBeTakenDescription => "The sign is firmly attached to the wall. ";

    public string ExaminationDescription => "A sign mounted on the wall. It reads: 'Adventurer Training Academy - Escape Room Tutorial'. ";

    public string ReadDescription => "Adventurer Training Academy - Escape Room Tutorial";

    public override string NeverPickedUpDescription(ILocation? currentLocation)
    {
        return string.Empty; // Don't list separately - it's mentioned in the room description
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A sign on the wall";
    }
}
