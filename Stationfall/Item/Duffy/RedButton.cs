using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     The emergency beacon button below the truck's viewport (ship.zil RED-BUTTON-F). It does nothing
///     at all until the flight is over — and the joke is that by the time pressing it is permitted, the
///     recording it plays is no comfort whatsoever.
/// </summary>
public class RedButton : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching =>
        ["red button", "button", "beacon", "emergency beacon", "distress beacon"];

    public string ExaminationDescription =>
        "A red button set into the console below the viewport, labelled EMERGENCY MESSAGE BEACON in " +
        "lettering considerably calmer than the situation it exists for. ";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override string CannotBeTakenDescription => "The button is part of the console. ";

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchNounAndAdjective(NounsForMatching))
            return new NoNounMatchInteractionResult();

        if (action.MatchVerb(["push", "press", "hit", "punch", "activate", "turn on"]))
            return new PositiveInteractionResult(Press());

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    /// <summary>
    ///     Gated on the flight being over rather than on having arrived safely: the original checks only
    ///     that the trip has finished (ship.zil RED-BUTTON-F against SPACETRUCK-COUNTER 5), which is
    ///     what lets a player stranded at the end of a wrong course actually reach the beacon — and get
    ///     that recording for an answer.
    /// </summary>
    private static string Press()
    {
        if (!Repository.GetLocation<Spacetruck>().FlightHasEnded)
            return "You're not in trouble! Misuse of the emergency message beacon is a court-martial " +
                   "offense. ";

        return "A recording answers, in the unhurried tone of someone reading from a card: \"At the " +
               "conclusion of this recording, your emergency message will be sent. In the meantime, " +
               "please remain calm. Nothing can go wrong... go wrong... go wrong... go wrong...\" ";
    }
}
