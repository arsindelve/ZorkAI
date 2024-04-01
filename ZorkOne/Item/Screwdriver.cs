namespace ZorkOne.Item;

public class Screwdriver : ItemBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["screwdriver"];

    public string ExaminationDescription => "There's nothing special about the screwdriver. ";

    public string OnTheGroundDescription => "There is a screwdriver here. ";
    
    public override string NeverPickedUpDescription => OnTheGroundDescription;
}