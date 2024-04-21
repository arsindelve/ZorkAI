namespace ZorkOne.Item;

public class BloodyAxe : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, IWeapon
{
    public override string[] NounsForMatching => ["axe", "bloody axe"];

    public override string InInventoryDescription => "A bloody axe ";

    public override string? CannotBeTakenDescription
    {
        get
        {
            var troll = Repository.GetItem<Troll>();
            if (CurrentLocation == troll && !troll.IsUnconscious)
                return "The troll swings it out of your reach.";

            return null;
        }
    }

    public string ExaminationDescription => "There's nothing special about the bloody axe. ";

    string ICanBeTakenAndDropped.OnTheGroundDescription => "There is a bloody axe here. ";

    public override string NeverPickedUpDescription => ((ICanBeTakenAndDropped)this).OnTheGroundDescription;
}