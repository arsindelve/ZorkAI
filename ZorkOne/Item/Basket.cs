namespace ZorkOne.Item;

public class Basket : ContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["basket", "cage"];

    public override string CannotBeTakenDescription => "The cage is securely fastened to the iron chain. ";

    protected override int SpaceForItems => 3;

    public override string Name => "basket";

    public string ExaminationDescription => Items.Any() ?  ItemListDescription("basket") : "The basket is empty. ";

    public override string NeverPickedUpDescription => ExaminationDescription;
    
    public override void Init()
    {
        // Empty in the beginning. 
    }
    
    public override bool IsTransparent => true;
}