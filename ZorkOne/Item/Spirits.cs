using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Spirits : ItemBase, ICanBeExamined, ITurnBasedActor, IPluralNoun
{
    [UsedImplicitly]
    public bool Stunned { get; set; }

    [UsedImplicitly]
    public int StunnedCounter { get; set; }

    /// <summary>
    ///     Phase two of the exorcism: the candles have been raised, lit, before the stunned spirits.
    ///     This latches (the XC global in ZIL) - once it is set the black book will banish them for
    ///     the remaining turns of the ceremony, whatever becomes of the candles in the meantime.
    /// </summary>
    [UsedImplicitly]
    public bool CandlePhaseComplete { get; set; }

    public override string[] NounsForMatching => ["spirits", "ghosts", "wraiths"];

    public override string CannotBeTakenDescription => ExaminationDescription;

    public string ExaminationDescription => "You seem unable to interact with these spirits. ";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!Stunned)
        {
            context.RemoveActor(this);
            return Task.FromResult("");
        }

        StunnedCounter--;

        if (StunnedCounter == 0)
        {
            Stunned = false;
            CandlePhaseComplete = false;
            var hades = Repository.GetLocation<EntranceToHades>();
            if (
                context.CurrentLocation == hades
                && Repository.GetItem<Spirits>().CurrentLocation == hades
            )
                return Task.FromResult(
                    "\nThe tension of this ceremony is broken, and the wraiths, amused but shaken at your clumsy attempt, resume their hideous jeering."
                );
        }

        return Task.FromResult("");
    }

    public string BecomeStunned(IContext context)
    {
        context.RegisterActor(this);
        Stunned = true;
        CandlePhaseComplete = false;

        // Six turns to get the candles back up and lit - the bell knocked them out of the
        // adventurer's hands, and re-taking them, striking a match and lighting them is three
        // turns of the six. The original queues I-XB for 6 here (zork1/1actions.zil:1101); the
        // extra tick is because this counter is decremented at the end of the ringing turn too.
        StunnedCounter = 7;

        return "The wraiths, as if paralyzed, stop their jeering and slowly turn to face you. "
               + "On their ashen faces, the expression of a long-forgotten terror takes shape. ";
    }

    /// <summary>
    ///     Phase two: lit candles have been raised before the stunned spirits. Latches
    ///     <see cref="CandlePhaseComplete" /> and restarts the clock, giving the adventurer three
    ///     turns to read the book. Returns the empty string if the ceremony is not at this point.
    /// </summary>
    public string RaiseTheCandles(IContext context)
    {
        // ZIL guards this transition with XB, the candles being carried AND lit, and NOT XC
        // (zork1/1actions.zil:1116-1119), so it announces itself exactly once per ceremony and
        // never for candles burning on the ground. Announcing it in states where the book will
        // not actually banish would tell the player the ceremony had advanced when it had not.
        if (!Stunned || CandlePhaseComplete)
            return string.Empty;

        if (context.CurrentLocation != Repository.GetLocation<EntranceToHades>())
            return string.Empty;

        if (CurrentLocation != Repository.GetLocation<EntranceToHades>())
            return string.Empty;

        var candles = Repository.GetItem<Candles>();
        if (!context.HasItem<Candles>() || !candles.IsOn)
            return string.Empty;

        CandlePhaseComplete = true;

        // Three turns to read the book, replacing whatever is left of the bell's window - the
        // original disables I-XB and queues I-XC for 3 (zork1/1actions.zil:1123-1124).
        StunnedCounter = 4;

        return "\nThe flames flicker wildly and appear to dance. The earth beneath your feet trembles, "
               + "and your legs nearly buckle beneath you. The spirits cower at your unearthly power. \n ";
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(
        SimpleIntent action,
        IContext context,
        IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory
        )
    {
        if (action.MatchNoun(NounsForMatching))
            return new PositiveInteractionResult(ExaminationDescription);

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}