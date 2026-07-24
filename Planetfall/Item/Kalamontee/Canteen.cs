namespace Planetfall.Item.Kalamontee;

public class Canteen : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["canteen", "octagonally shaped canteen"];

    public override Type[] CanOnlyHoldTheseTypes => [typeof(ProteinLiquid)];

    // Issue #422: the canteen-holds-only-protein-liquid restriction is a port addition (the original
    // CANTEEN has no ACTION routine guarding PUT). It is kept deliberately, but a bare type rejection
    // returns no message and falls through to the AI narrator in MultiNounEngine - a blank line when
    // generation returns nothing. Naming the item keeps the refusal deterministic.
    public override string CanOnlyHoldTheseTypesErrorMessage(string nameOfItemWeTriedToPlace) =>
        $"The {nameOfItemWeTriedToPlace} won't fit through the mouth of the canteen. ";

    public override int Size => 1;

    public override bool IsTransparent => false;

    public string ExaminationDescription => Items.Any()
        ? ItemListDescription("canteen", null)
        : $"The canteen is {(IsOpen ? "open" : "closed")}. ";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Although the room is quite barren, an octagonally shaped canteen is sitting on one of the benches. ";
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a canteen here. " + (Items.Any() && IsOpen ? $"\n{ItemListDescription("canteen", null)}" : "");
    }

    public override void Init()
    {
    }

    public override string NowOpen(ILocation currentLocation)
    {
        // Empty must fall back to the base "Opened." - OnOpening only supplies text when
        // there's protein liquid to describe, so returning empty here for both would leave
        // the player with a blank response (issue #370).
        return Items.Any() ? string.Empty : base.NowOpen(currentLocation);
    }

    public override string OnOpening(IContext context)
    {
        if (Items.Any())
            return "Opening the canteen reveals a quantity of protein-rich liquid. ";

        return base.OnOpening(context);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A canteen" +
               (Items.Any() && IsOpen ? $"\n{ItemListDescription("canteen", null)}" : "");
    }
}