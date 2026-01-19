using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace EscapeRoom.Item;

public class WelcomeDesk : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string? CannotBeTakenDescription => "The desk is too heavy to move. ";

    public override string[] NounsForMatching => ["desk", "welcome desk", "drawer"];

    public override string Name => "welcome desk";

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen
            ? Items.Any()
                ? "A welcome desk with an open drawer. Inside is a leaflet. "
                : "A welcome desk with an open drawer. The drawer is empty. "
            : "A welcome desk with a closed drawer. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "There is a welcome desk here. ";
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? "Opening the desk drawer reveals a leaflet. " : "Opened. ";
    }

    public override void Init()
    {
        StartWithItemInside<Leaflet>();
    }

    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["search", "look in", "look inside"], NounsForMatching))
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(ExaminationDescription));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
