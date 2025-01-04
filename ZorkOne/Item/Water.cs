using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.Item;

public class Water : ItemBase, IAmADrink
{
    public override string[] NounsForMatching => ["water", "quantity of water"];

    public override string CannotBeTakenDescription => "It would just slip through your fingers";

    public override int Size => 2;

    string IAmADrink.OnDrinking(IContext context)
    {
        return "Thank you very much -- I was very thirsty (probably from all this talking).";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A quantity of water";
    }
}