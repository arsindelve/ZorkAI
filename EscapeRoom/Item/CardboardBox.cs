using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace EscapeRoom.Item;

public class CardboardBox : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string? CannotBeTakenDescription => "The box is too bulky to carry around. ";

    public override string[] NounsForMatching => ["box", "cardboard box", "cardboard"];

    public override string Name => "cardboard box";

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen
            ? Items.Any()
                ? "A dusty cardboard box, now open. Inside you can see a brass key. "
                : "The box is open and empty. "
            : "A dusty cardboard box. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "There is a cardboard box here. ";
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? "Opening the cardboard box reveals a brass key. " : "Opened. ";
    }

    public override void Init()
    {
        StartWithItemInside<BrassKey>();
    }

    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["search", "look in", "look inside"], NounsForMatching))
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(ExaminationDescription));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
