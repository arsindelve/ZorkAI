using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Newtonsoft.Json;

namespace ZorkOne.Item;

public class Machine : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["machine", "dryer"];

    protected override int SpaceForItems => 6;

    public override string NowOpen =>
        Items.Any() ? $"The lid opens revealing {SingleLineListOfItems()}. " : "The lid opens. ";

    public override string NowClosed => "The lid closes.";

    public override string CannotBeTakenDescription => "It is far too large to carry. ";

    [JsonIgnore]
    string ICanBeExamined.ExaminationDescription => IsOpen
        ? Items.Any() ? base.ItemListDescription("machine") : "The machine is empty. "
        : "The machine is closed. ";

    public string OnTheGroundDescription => string.Empty;

    public override void Init()
    {
        // Initially empty. 
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (!IsOpen && action.Match(["open"], ["lid"]))
        {
            IsOpen = true;
            return new PositiveInteractionResult(NowOpen);
        }

        if (IsOpen && action.Match(["close"], ["lid"]))
        {
            IsOpen = false;
            return new PositiveInteractionResult(NowClosed);
        }

        if (action.Verb.ToLowerInvariant() == "turn" && action.Noun?.ToLowerInvariant() == "switch")
            return new PositiveInteractionResult("Your bare hands don't appear to be enough.");

        return base.RespondToSimpleInteraction(action, context, client);
    }

 
}