using Model.AIGeneration;

namespace Planetfall.Item.Lawanda;

internal class MedicineBottle : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeRead
{
    public override string[] NounsForMatching => ["medicine bottle", "bottle", "vial"];

    public override Type[] CanOnlyHoldTheseTypes => [typeof(Medicine)];

    // Issue #422: the bottle-holds-only-medicine restriction is a port addition (the original
    // MEDICINE-BOTTLE has no ACTION routine guarding PUT). It is kept deliberately, but a bare type
    // rejection returns no message and falls through to the AI narrator in MultiNounEngine - a blank
    // line when generation returns nothing. Naming the item keeps the refusal deterministic.
    public override string CanOnlyHoldTheseTypesErrorMessage(string nameOfItemWeTriedToPlace) =>
        $"The {nameOfItemWeTriedToPlace} won't fit in the bottle. ";

    public override bool IsTransparent => true;

    // Issue #514: do NOT re-implement ICanBeExamined here. The engine normalizes "look in <noun>" /
    // "look inside <noun>" to "examine <noun>" (GameEngine.NormalizeLookAt, issue #396), so the examine
    // text IS this container's contents listing. Aliasing ExaminationDescription to ReadDescription made
    // both verbs print the label - byte-identical whether the bottle was full or empty, the one container
    // in the game that would not say what was inside it. Inheriting OpenAndCloseContainerBase's explicit
    // ICanBeExamined member gives the sibling-container behavior: "The medicine bottle contains: ..." or
    // "The medicine bottle is empty." (always visible - the bottle is transparent). The label stays on
    // the READ path below and on "read/examine label" in RespondToSimpleInteraction.
    public string ReadDescription => "\"Dizeez supreshun medisin -- eksperimentul\"";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a medicine bottle here. " + Environment.NewLine +
               (Items.Any()
                   ? Repository.GetItem<Medicine>().NeverPickedUpDescription(currentLocation)
                   : "");
    }

    // Issue #116: drinking the medicine is one-shot - the engine destroys the Medicine on consumption,
    // so Items.Any() goes false and the descriptions above stop mentioning the contents (empty bottle).

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "On a low shelf is a translucent bottle with a small white label. "
               + Environment.NewLine +
               (Items.Any()
                   ? Repository.GetItem<Medicine>().NeverPickedUpDescription(currentLocation)
                   : "");
    }

    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["read", "examine"], ["label"]))
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(ReadDescription));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override void Init()
    {
        StartWithItemInside<Medicine>();
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A medicine bottle" + (Items.Any() ? $"\n{ItemListDescription("medicine bottle", currentLocation)}" : "");
    }
}