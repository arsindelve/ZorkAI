namespace ZorkOne.Item;

public class Trident : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["trident", "crystal trident", "poseidon's trident", "poseidon's own trident"];

    public override string InInventoryDescription => "A crystal trident";

    public string OnTheGroundDescription => "There is a crystal trident here. ";

    public override string NeverPickedUpDescription => "On the shore lies Poseidon's own crystal trident. ";

    public override int Size => 9;

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 4;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 11;
}