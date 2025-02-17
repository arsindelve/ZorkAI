using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Newtonsoft.Json;

namespace ZorkOne.Item;

public class Machine : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["machine", "dryer"];

    protected override int SpaceForItems => 6;

    public override string CannotBeTakenDescription => "It is far too large to carry. ";

    [JsonIgnore]
    string ICanBeExamined.ExaminationDescription => IsOpen
        ? Items.Any() ? base.ItemListDescription("machine", null) : "The machine is empty. "
        : "The machine is closed. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? $"The lid opens revealing {SingleLineListOfItems()}. " : "The lid opens. ";
    }

    public override string NowClosed(ILocation currentLocation)
    {
        return "The lid closes.";
    }

    public override void Init()
    {
        // Initially empty. 
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!IsOpen && action.Match(["open"], ["lid"]))
        {
            IsOpen = true;
            return new PositiveInteractionResult(NowOpen(context.CurrentLocation));
        }

        if (IsOpen && action.Match(["close"], ["lid"]))
        {
            IsOpen = false;
            return new PositiveInteractionResult(NowClosed(context.CurrentLocation));
        }

        if (action.Verb.ToLowerInvariant() == "turn" && action.Noun?.ToLowerInvariant() == "switch")
            return new PositiveInteractionResult("Your bare hands don't appear to be enough.");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}