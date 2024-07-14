using Model.Interface;

namespace ZorkOne.Item;

public class RustyKnife : ItemBase, ICanBeTakenAndDropped, IWeapon
{
    public override string InInventoryDescription => "a rusty knife";

    public override string[] NounsForMatching => ["knife", "rusty knife", "rusty"];

    string ICanBeTakenAndDropped.OnTheGroundDescription => "TThere is a rusty knife here. ";

    string ICanBeTakenAndDropped.NeverPickedUpDescription =>
        "Beside the skeleton is a rusty knife. ";

    public override string? OnBeingTaken(IContext context)
    {
        return context.HasItem<Sword>() 
            ? "As you touch the rusty knife, your sword gives a single pulse of blinding blue light. " 
            : base.OnBeingTaken(context);
    }
}