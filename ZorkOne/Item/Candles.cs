using System.Diagnostics;
using GameEngine;
using GameEngine.Item;
using GameEngine.Item.ItemProcessor;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Candles
    : ItemBase,
        ICanBeExamined,
        ICanBeTakenAndDropped,
        IAmALightSourceThatTurnsOnAndOff,
        ITurnBasedActor,
        IPluralNoun
{
    [UsedImplicitly]
    public int TurnsWhileOn { get; set; }

    [UsedImplicitly]
    public bool BurnedOut { get; set; }

    public override string[] NounsForMatching => ["candle", "candles", "pair of candles"];

    public bool IsOn { get; set; }

    public string NowOnText => "The candles are lit. ";

    public string NowOffText => "The flame is extinguished. ";

    public string AlreadyOffText => "The candles are not lighted. ";

    public string AlreadyOnText => "The candles are already lit.";

    public string CannotBeTurnedOnText
    {
        // This handles the case where the adventurer has not specified what to light them with
        // If they have a burning match, go ahead and make that assumption. Otherwise, let them know
        // they are going to have to be more specific. When they do specify what to light them with,
        // it will be handled by the multi-noun process, not this. 
        get
        {
            if (BurnedOut)
                return "Alas, there's not much left of the candles. Certainly not enough to burn. ";

            var matches = Repository.GetItem<Matchbook>();
            if (matches.CurrentLocation?.Name == "Inventory" && matches.IsOn)
                return string.Empty;

            return "You should say what to light them with. ";
        }
    }

    public string OnBeingTurnedOn(IContext context)
    {
        context.RegisterActor(this);

        var location = Repository.GetLocation<EntranceToHades>();
        var spirits = Repository.GetItem<Spirits>();

        if (
            context.CurrentLocation == location
            && spirits.CurrentLocation == location
            && spirits.Stunned
        )
            return "\nThe flames flicker wildly and appear to dance. The earth beneath your feet trembles, "
                   + "and your legs nearly buckle beneath you. The spirits cower at your unearthly power. \n ";

        return string.Empty;
    }

    public void OnBeingTurnedOff(IContext context)
    {
        context.RemoveActor(this);
    }

    public string ExaminationDescription =>
        IsOn ? "The candles are burning. " : "The candles are out. ";

    public override string OnBeingTaken(IContext context)
    {
        // Once we take the candles for the first time, they
        // start to get shorter. I guess they were magic before
        // that?
        context.RegisterActor(this);
        return string.Empty;
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a pair of candles here" + (IsOn ? " (providing light). " : ". ");
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "On the two ends of the altar are burning candles. ";
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!IsOn)
        {
            context.RemoveActor(this);
            return Task.FromResult(string.Empty);
        }

        Debug.WriteLine($"Candles counter: {TurnsWhileOn}");
        TurnsWhileOn++;

        switch (TurnsWhileOn)
        {
            case 13:
                return Task.FromResult("The candles grow shorter.");

            case 20:
                return Task.FromResult("The candles are becoming quite short.");

            case 24:
                return Task.FromResult("The candles won't last long now.");

            case 26:
                BurnedOut = true;
                var result = new TurnOnOrOffProcessor().Process(
                    new SimpleIntent { Noun = NounsForMatching.First(), Verb = "turn off" },
                    context,
                    this,
                    client
                );
                return Task.FromResult(
                    "You'd better have more light than from the pair of candles. "
                    + result!.InteractionMessage
                );

            default:
                return Task.FromResult(string.Empty);
        }
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A pair of candles" + (IsOn ? " (providing light)" : "");
    }

    public override InteractionResult RespondToMultiNounInteraction(
        MultiNounIntent action,
        IContext context
        )
    {
        if (!action.Match(
                ["light", "burn"],
                NounsForMatching,
                Repository.GetItem<Matchbook>().NounsForMatching.Union(Repository.GetItem<Torch>().NounsForMatching)
                    .ToArray(),
                ["with"]
            ))
            return base.RespondToMultiNounInteraction(action, context);

        if (action.MatchNounTwo<Torch>() && context.HasItem<Torch>())
        {
            Repository.DestroyItem(this);
            return new PositiveInteractionResult(
                "The heat from the torch is so intense that the candles are vaporized. ");
        }

        if (action.MatchNounTwo<Matchbook>() && context.HasItem<Matchbook>())
        {
            if (!Repository.GetItem<Matchbook>().IsOn)
                return new PositiveInteractionResult("You'll need to light a match first. ");

            return new TurnOnOrOffProcessor().Process(
                new SimpleIntent { Noun = action.NounOne, Verb = action.Verb },
                context,
                this,
                null!
            )!;
        }

        return base.RespondToMultiNounInteraction(action, context);
    }
}