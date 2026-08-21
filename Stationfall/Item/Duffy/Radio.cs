using Newtonsoft.Json;
using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     The truck's space-band radio (ship.zil RADIO-F, interrupts.zil I-RADIO). Its microphone is
///     missing, so it receives and never transmits: you can listen to the lanes but you cannot answer,
///     which is the point of it. Switched on, it chatters occasionally while you are aboard.
/// </summary>
public class Radio : ItemBase, ICanBeExamined, ITurnBasedActor
{
    /// <summary>
    ///     Percentage chance per turn of a snatch of traffic, checked first (I-RADIO).
    /// </summary>
    private const int ChanceOfChatter = 30;

    /// <summary>
    ///     Percentage chance, when no traffic came through, of a music station drifting past instead.
    /// </summary>
    private const int ChanceOfMusic = 20;

    [UsedImplicitly] [JsonIgnore] public IRandomChooser Chooser { get; set; } = new RandomChooser();

    [UsedImplicitly] public bool IsOn { get; set; }

    public override string[] NounsForMatching =>
        ["radio", "sb radio", "space band radio", "spaceband radio", "space-band radio"];

    public string ExaminationDescription =>
        "A space-band radio built into the console. It seems to be damaged: the microphone is missing, " +
        $"so you can listen but never answer. It is currently {(IsOn ? "on" : "off")}. ";

    public override string CannotBeTakenDescription => "The radio is built into the console. ";

    /// <summary>
    ///     Chatter written for this port. The originals are trucker-radio jokes transplanted to
    ///     spacelanes; these are the same idea in different words.
    /// </summary>
    private static readonly List<string> Chatter =
    [
        "Keep it under twenty-six thousand kilometers per millichron through lane 630-461, people. " +
        "There's a patrol cutter parked behind the third beacon.",
        "Anybody running the Nebulon sector tonight? Looking for a word on the inspection posts.",
        "Somebody give me a traffic report on lane 317-455 before I commit to it.",
        "Second time this shift I've been routed around the same dead beacon. Somebody file a form.",
        "If the outfit hauling nine hundred tons of blank requisition slips is listening: your load is " +
        "not secured, and I have the paperwork to prove it."
    ];

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchNounAndAdjective(NounsForMatching))
            return new NoNounMatchInteractionResult();

        if (action.MatchVerb(["listen", "listen to", "hear"]))
            return new PositiveInteractionResult(
                IsOn ? "\"Hiss. Crackle.\" " : "The radio isn't on! ");

        if (action.MatchVerb(["turn on", "switch on", "activate", "start"]))
        {
            if (IsOn)
                return new PositiveInteractionResult("It's already on. ");

            IsOn = true;
            context.RegisterActor(this);
            return new PositiveInteractionResult("The radio comes on with a wash of static. ");
        }

        if (action.MatchVerb(["turn off", "switch off", "deactivate", "stop", "silence"]))
        {
            if (!IsOn)
                return new PositiveInteractionResult("It's already off. ");

            IsOn = false;
            context.RemoveActor(this);
            return new PositiveInteractionResult("You switch the radio off. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    /// <summary>
    ///     Chatter only reaches you in the cab. The original checks the player's room rather than the
    ///     radio's, so a radio left on and left behind is silent — which is also what stops it talking
    ///     over the rest of the game once you have walked away from the truck.
    /// </summary>
    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!IsOn || context.CurrentLocation is not Spacetruck)
            return Task.FromResult(string.Empty);

        if (Chooser.RollDice(100) <= ChanceOfChatter)
            return Task.FromResult($"The radio crackles to life. \"Breaker. {Chooser.Choose(Chatter)} Over.\" ");

        if (Chooser.RollDice(100) <= ChanceOfMusic)
            return Task.FromResult(
                "A country and western station drifts into tune for a moment, long enough for someone " +
                "to be badly wronged in three-quarter time, and then fades out again. ");

        return Task.FromResult(string.Empty);
    }
}
