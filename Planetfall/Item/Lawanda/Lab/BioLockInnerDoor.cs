using Planetfall.Command;

namespace Planetfall.Item.Lawanda.Lab;

internal class BioLockInnerDoor : SimpleDoor
{
    public override string[] NounsForMatching =>
    [
        "door"
    ];

    public override string OnOpening(IContext context)
    {
        var youDie =
            "Opening the door reveals a Bio-Lab full of horrible mutations. You stare at them, frozen with horror. " +
            "Growling with hunger and delight, the mutations march into the bio-lock and devour you.";

        return new DeathProcessor().Process(youDie, context).InteractionMessage;
    }
}