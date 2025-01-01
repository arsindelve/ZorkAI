namespace Planetfall.Item.Feinstein;

public class Uniform : ContainerBase, ICanBeTakenAndDropped, ICanBeExamined, IAmClothing
{
    public override string[] NounsForMatching => ["patrol uniform", "uniform"];

    public string ExaminationDescription => "It is a standard-issue one-pocket Stellar Patrol uniform, a miracle of modern technology. It will keep its owner warm in cold climates and cool in warm locales. It provides protection against mild radiation, repels all insects, absorbs sweat, promotes healthy skin tone, and on top of everything else, it is supercomfy. ";

    // Starts the game being worn. 
    public bool BeingWorn { get; set; } = true;
    
    public override void Init()
    {
        StartWithItemInside<IdCard>();
    }

    // This just means that we can always see what is in the container. It does not open and close. 
    public override bool IsTransparent => true;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A slightly wrinkled Patrol uniform is lying here. " + (Items.Any() ? $"\n{ItemListDescription("Patrol uniform", null)}" : "");
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A Patrol uniform" + (BeingWorn ? " (being worn)" : "") +
               (Items.Any() ? $"\n{ItemListDescription("Patrol uniform", null)}" : "");
    }

    public override int Size => 1;
}
