using Model.AIGeneration;

namespace Planetfall.Item.Lawanda;

internal class MedicineBottle : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeRead, ICanBeExamined
{
    public override string[] NounsForMatching => ["medicine bottle", "bottle", "vial", "medicine"];

    public override Type[] CanOnlyHoldTheseTypes => [typeof(Medicine)];

    public override bool IsTransparent => true;
    public string ExaminationDescription => ReadDescription;

    public string ReadDescription => "\"Dizeez supreshun medisin -- eksperimentul\"";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a medicine bottle here. ";
    }

    // TODO: The medicine bottle is now empty.
    
    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "On a low shelf is a translucent bottle with a small white label. ";
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
        return "A medicine bottle";
    }
}