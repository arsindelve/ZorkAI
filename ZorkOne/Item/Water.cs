namespace ZorkOne.Item;

public class Water : ItemBase, ICanBeExamined, IAmADrink
{
    public override string[] NounsForMatching => ["water", "quantity of water"];

    public override string CannotBeTakenDescription => "It would just slip through your fingers";

    public override string InInventoryDescription => "A quantity of water";

    public string DrankDescription => "Thank you very much -- I was very thirsty (probably from all this talking).";

    string ICanBeExamined.ExaminationDescription => "There's nothing special about the quantity of water.";
}