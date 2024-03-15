namespace ZorkOne.Item;

public class Water : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, IAmADrink
{
    public override string[] NounsForMatching => ["water", "quantity of water"];

    public override string InInventoryDescription => "A quantity of water";

    string ICanBeExamined.ExaminationDescription => "There's nothing special about the quantity of water.";

    string ICanBeTakenAndDropped.OnTheGroundDescription => string.Empty;
    
    public string DrankDescription => "Thank you very much -- I was very thirsty (probably from all this talking).";
}