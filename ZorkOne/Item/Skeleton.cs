namespace ZorkOne.Item;

public class Skeleton : ItemBase
{
    public override string[] NounsForMatching => ["skeleton", "remains", "adventurer", "luckless adventurer"];

    public override string NeverPickedUpDescription =>
        "A skeleton, probably the remains of a luckless adventurer, lies here. ";

    public override string? CannotBeTakenDescription =>
        "A ghost appears in the room and is appalled at your desecration of the remains of a fellow adventurer. " +
        //TODO: "He casts a curse on your valuables and banishes them to the Land of the Living Dead. " +
        "The ghost leaves, muttering obscenities. ";
}

public class BurnedOutLantern : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["lantern", "busted", "busted lantern"];
    
    public string OnTheGroundDescription => "There is a burned-out lantern here. ";

    public override string NeverPickedUpDescription => "The deceased adventurer's useless lantern is here. ";
}
