using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.Item;

public class Water : ItemBase, IAmADrink
{
    public override string[] NounsForMatching => ["water", "quantity of water"];

    public override string CannotBeTakenDescription => "The water slips through your fingers.";

    public override int Size => 2;

    (string Message, bool WasConsumed) IAmADrink.OnDrinking(IContext context)
    {
        return ("Thank you very much. I was rather thirsty (from all this talking, probably).", true);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A quantity of water ";
    }
}
