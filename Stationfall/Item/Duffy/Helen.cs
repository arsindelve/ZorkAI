using GameEngine.IntentEngine;

namespace Stationfall.Item.Duffy;

/// <summary>
///     Helen — a form-sorting robot in bin two, and the other wrong answer. She is harmless to the
///     player but fatal to paperwork: given a form, she bursts and decollates it into confetti
///     (ship.zil:806-816, 832-836, 889-893), which can destroy the activation form you need to leave.
/// </summary>
public class Helen : ShipRobot, ICanBeGivenThings
{
    /// <summary>
    ///     "give X to Helen" isn't routed automatically; a recipient wires it up itself, the same way
    ///     Planetfall's Floyd does.
    /// </summary>
    private readonly GiveSomethingToSomeoneDecisionEngine<Helen> _giveEngine = new();

    public override int BinNumber => 2;

    public override string[] NounsForMatching => ["helen", "spindly robot", "small robot"];

    public override string InTheBinDescription =>
        "The second bin holds a spindly little robot bristling with perforating attachments, built to " +
        "burst and decollate multi-part forms. A tiny nameplate reads \"Helen.\" ";

    public override string ExaminationDescription =>
        "Helen is a slender robot of many delicate arms, every one of them designed to separate one " +
        "piece of paper from another. She eyes your forms with unmistakable professional interest. ";

    /// <summary>
    ///     Hand Helen a form and it is destroyed — permanently, and without warning.
    /// </summary>
    public InteractionResult OfferThisThing(IItem item, IContext context)
    {
        if (item is not FormBase form)
            return new PositiveInteractionResult(
                "Helen inspects it for perforations, finds none, and hands it back. ");

        return new PositiveInteractionResult(Shred(form, context));
    }

    /// <summary>
    ///     Destroys a form: it leaves the game entirely, exactly as the original's CONFETTI does.
    /// </summary>
    public static string Shred(FormBase form, IContext context)
    {
        var title = $"{form.FormTitle} Form {form.FormNumber}";

        form.Consumed = true;
        context.RemoveItem(form);
        form.CurrentLocation?.RemoveItem(form);
        form.CurrentLocation = null;

        return $"In a spasm of vocational enthusiasm, Helen bursts and decollates your {title}, " +
               "leaving nothing but a drift of useless confetti. ";
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        var given = _giveEngine.AreWeGivingSomethingToSomeone(action, this, context);

        if (given is not null)
            return given;

        return await base.RespondToMultiNounInteraction(action, context);
    }

    protected override string FollowThePlayer(IContext context)
    {
        MoveToPlayer(context);
        return "Helen follows you, sorting the air with idle little snips. ";
    }
}
