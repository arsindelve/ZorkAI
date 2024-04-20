using Model.AIGeneration;
using Model.Intent;

namespace ZorkOne.Item;

public class Candles : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, IAmALightSource, ICanBeTurnedOnAndOff
{
    public bool Lit { get; set; }

    public override string[] NounsForMatching => ["candle", "candles"];

    public override string InInventoryDescription => "A pair of candles" + (Lit ? " (providing light)" : "");

    public string ExaminationDescription => Lit ? "The candles are burning. " : "The candles are out. ";

    public string OnTheGroundDescription => "There is a pair of candles here" + (Lit ? " (providing light). " : ".");

    public override string NeverPickedUpDescription => "On the two ends of the altar are burning candles. ";

    public bool IsOn
    {
        get => Lit;
        set => Lit = value;
    }

    public string NowOnText { get; }
    public string NowOffText => "The flame is extinguished. ";
    public string AlreadyOffText { get; }
    public string AlreadyOnText { get; }

    public string? CannotBeTurnedOnText => "You should say what to light them with. ";

    public void OnBeingTurnedOn(IContext context)
    {
    }

    public void OnBeingTurnedOff(IContext context)
    {
    }

    // TODO: Candles burn down. 
    
    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client)
    {
        string[] verbs = ["blow out", "blow", "extinguish"];

        if (action.Match(verbs, NounsForMatching))
        {
            Lit = false;
            return new PositiveInteractionResult("The flame is extinguished.");
        }
        
        return base.RespondToSimpleInteraction(action, context, client);
    }
}