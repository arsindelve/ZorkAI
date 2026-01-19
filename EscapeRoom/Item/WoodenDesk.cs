using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace EscapeRoom.Item;

public class WoodenDesk : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string? CannotBeTakenDescription => "The desk is too heavy to move. ";

    public override string[] NounsForMatching => ["desk", "wooden desk", "drawer", "office desk"];

    public override string Name => "wooden desk";

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen
            ? Items.Any()
                ? "A sturdy wooden desk with an open drawer. Inside is a flashlight. "
                : "A sturdy wooden desk with an open drawer. The drawer is empty. "
            : "A sturdy wooden desk with a closed drawer. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "There is a wooden desk here. ";
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? "Opening the desk drawer reveals a flashlight. " : "Opened. ";
    }

    public override void Init()
    {
        StartWithItemInside<Flashlight>();
    }

    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["search", "look in", "look inside"], NounsForMatching))
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(ExaminationDescription));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
