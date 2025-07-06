using GameEngine.Item;

namespace ZorkOne.Item;

public class Basket : ContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["basket", "cage"];

    public override string CannotBeTakenDescription => "The cage is securely fastened to the iron chain. ";

    protected override int SpaceForItems => 12;

    public override string Name => "basket";

    public override bool IsTransparent => true;

    public string ExaminationDescription => Items.Any() ? ItemListDescription("basket", null) : "The basket is empty. ";

    public override void Init()
    {
        // Empty in the beginning. 
    }
}