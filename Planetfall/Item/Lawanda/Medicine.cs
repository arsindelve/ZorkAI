namespace Planetfall.Item.Lawanda;

internal class Medicine : ItemBase, IAmADrink
{
    public override string[] NounsForMatching => ["medicine"];

    public (string Message, bool WasConsumed) OnDrinking(IContext context)
    {
        // Issue #116: the experimental disease-suppression medicine (comptwo.zil:170-185) rolls the
        // sickness clock back two levels, which also restores 20 carrying capacity (EffectiveLoadAllowed is
        // derived from the counter). It's a one-shot stopgap: returning WasConsumed = true makes the engine
        // destroy the medicine, emptying the bottle so a second drink finds nothing.
        if (context is PlanetfallContext pc)
            pc.SicknessCounter = Math.Max(1, pc.SicknessCounter - 2);

        return ("The medicine tasted extremely bitter. ", true);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A quantity of medicine";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "At the bottom of the bottle is a small quantity of medicine. ";
    }
}