using System.Diagnostics;
using Game.Item.ItemProcessor;
using Model.AIGeneration;
using Model.Intent;
using ZorkOne.Location;

namespace ZorkOne.Item;

public class Candles : ItemBase, ICanBeExamined, ICanBeTakenAndDropped,
    IAmALightSourceThatTurnsOnAndOff, ITurnBasedActor
{
    public int TurnsWhileOn { get; set; }

    public bool BurnedOut { get; set; }

    public override string[] NounsForMatching => ["candle", "candles", "pair of candles"];

    public override string InInventoryDescription => "A pair of candles" + (IsOn ? " (providing light)" : "");

    public bool IsOn { get; set; }

    public string NowOnText => "The candles are lit. ";
    public string NowOffText => "The flame is extinguished. ";
    public string AlreadyOffText => "The candles are not lighted.";
    public string AlreadyOnText => "The candles are already lit.";

    public string CannotBeTurnedOnText
    {
        get
        {
            if (BurnedOut)
                return "Alas, there's not much left of the candles. Certainly not enough to burn.";

            var matches = Repository.GetItem<Matchbook>();
            if (matches.CurrentLocation!.Name == "Inventory" && matches.IsOn)
                return string.Empty;

            return "You should say what to light them with. ";
        }
    }

    public string OnBeingTurnedOn(IContext context)
    {
        context.RegisterActor(this);

        var location = Repository.GetLocation<EntranceToHades>();
        var spirits = Repository.GetItem<Spirits>();

        if (context.CurrentLocation == location &&
            spirits.CurrentLocation == location &&
            spirits.Stunned)
        {
            return
                "\nThe flames flicker wildly and appear to dance. The earth beneath your feet trembles, " +
                "and your legs nearly buckle beneath you. The spirits cower at your unearthly power. \n ";
        }

        return string.Empty;
    }

    public void OnBeingTurnedOff(IContext context)
    {
        context.RemoveActor(this);
    }

    public string ExaminationDescription => IsOn ? "The candles are burning. " : "The candles are out. ";

    public override string OnBeingTaken(IContext context)
    {
        // Once we take the candles for the first time, they
        // start to get shorter. I guess they were magic before
        // that? 
        context.RegisterActor(this);
        return string.Empty;
    }

    public string OnTheGroundDescription => "There is a pair of candles here" + (IsOn ? " (providing light). " : ".");

    public override string NeverPickedUpDescription => "On the two ends of the altar are burning candles. ";

    public string Act(IContext context, IGenerationClient client)
    {
        Debug.WriteLine($"Candles counter: {TurnsWhileOn}");
        TurnsWhileOn++;

        switch (TurnsWhileOn)
        {
            case 13:
                return "The candles grow shorter.";

            case 20:
                return "The candles are becoming quite short.";

            case 24:
                return "The candles won't last long now.";

            case 26:
                BurnedOut = true;
                var result = new TurnLightOnOrOffProcessor().Process(
                    new SimpleIntent { Noun = NounsForMatching.First(), Verb = "turn off" }, context, this,
                    client);
                return "You'd better have more light than from the pair of candles. " + result!.InteractionMessage;

            default:
                return string.Empty;
        }
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        if (action.Match(["light", "burn"], NounsForMatching,
                Repository.GetItem<Matchbook>().NounsForMatching,
                ["with"]))
            return new TurnLightOnOrOffProcessor().Process(new SimpleIntent
            {
                Noun = action.NounOne,
                Verb = action.Verb
            }, context, this, null!)!;

        return base.RespondToMultiNounInteraction(action, context);
    }
}