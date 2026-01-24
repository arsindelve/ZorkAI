using GameEngine.Location;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Location.Lawanda.LabOffice;

internal class CryoAnteroomLocation : LocationBase
{
    public override string Name => "Cryogenic Anteroom";

    #region Ending Text

    // Common opening for all endings
    private const string CommonOpening =
        "A door slides open and a medical robot glides in. It opens the cryo-unit and administers an injection to its inhabitant. As the robot glides away, a figure rises from the cryo-unit -- a handsome, middle-aged woman with flowing red hair. She spends some time studying readouts from the control panel, pressing several keys. ";

    // Victory opening (used when course control is fixed)
    private const string VictoryOpening =
        "As other cryo-units in the chambers beyond begin opening, the woman turns to you, bows gracefully, and speaks in a beautiful, lilting voice. \"I am Veldina, leader of Resida. Thanks to you, the cure has been discovered, and the planetary systems repaired. We are eternally grateful.\"";

    // Complete Victory additions
    private const string CompleteVictoryPart1 =
        "\"You will also be glad to hear that a ship of your Stellar Patrol now orbits the planet. I have sent them the coordinates for this room.\" As if on cue, a landing party from the S.P.S. Flathead materializes nearby. Blather is with them, having been picked up from deep space in another escape pod, babbling cravenly. Captain Sterling of the Flathead acknowledges your heroic actions, and informs you of your promotion to Lieutenant First Class.";

    private const string CompleteVictoryPart2 =
        "As a team of mutant hunters head for the cryo-elevator, Veldina mentions that the grateful people of Resida offer you leadership of their world. Captain Sterling points out that, even if you choose to remain on Resida, Blather (demoted to Ensign Twelfth Class) has been assigned as your personal toilet attendant.";

    private const string CompleteVictoryPart3 =
        "You feel a sting from your arm and turn to see a medical robot moving away after administering the antidote for The Disease.";

    private const string FloydReturns =
        "A team of robot technicians step into the anteroom. They part their ranks, and a familiar figure comes bounding toward you! \"Hi!\" shouts Floyd, with uncontrolled enthusiasm. \"Floyd feeling better now!\" Smiling from ear to ear, he says, \"Look what Floyd found!\" He hands you a helicopter key, a reactor elevator card, and a paddleball set. \"Maybe we can use them in the sequel...\"";

    // Stranded endings
    private const string StrandedDefenseFailed =
        "\"Unfortunately, a second ship from your Stellar Patrol has been destroyed by our malfunctioning meteor defenses. I fear that you are stranded on Resida, possibly forever. However, we show our gratitude by offering you an unlimited bank account and a house in the country.\"";

    private const string StrandedCommsFailed =
        "\"Unfortunately, a second ship from your Stellar Patrol has come looking for survivors, and because of our malfunctioning communications system, has given up and departed. I fear that you are stranded on Resida, possibly forever. However, we show our gratitude by offering you an unlimited bank account and a house in the country.\"";

    // Doomed planet endings
    private const string DoomedOpening =
        "She turns to you and, with a strained voice says, \"You have fixed our computer and a Cure has been discovered, and we are grateful. But alas, it was all in vain. Our planetary course control system has malfunctioned, and the orbit has now decayed beyond correction. Soon Resida will plunge into the sun.\"";

    private const string DoomedWithRescue =
        "Veldina examines the control panel again. \"Fortunately, another ship from your Stellar Patrol has arrived, so at least you will survive.\" At that moment, a landing party from the S.P.S. Flathead materializes, and takes you away from the doomed world.";

    #endregion

    public override void Init()
    {
        // No items in this location
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return string.Empty; // Description is handled by AfterEnterLocation
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // No exits - this is the end of the game
        return new Dictionary<Direction, MovementParameters>();
    }

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        // Game is over - generate a "you won" response for any input
        // Include a random seed to ensure varied responses each time
        var request = new WonTheGameRequest(Random.Shared.Next(1000));
        var response = await client.GenerateNarration(request, context.SystemPromptAddendum);
        return new PositiveInteractionResult(response);
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var systemsMonitor = Repository.GetLocation<SystemsMonitors>();

        string endingText = GetEndingText(
            systemsMonitor.CommunicationsFixed,
            systemsMonitor.PlanetaryDefenseFixed,
            systemsMonitor.CourseControlFixed);

        return Task.FromResult(endingText);
    }

    private static string GetEndingText(bool commFixed, bool defenseFixed, bool courseControlFixed)
    {
        // Ending 1: Complete Victory (All 3 Fixed)
        if (courseControlFixed && commFixed && defenseFixed)
        {
            return string.Join("\n\n",
                CommonOpening,
                VictoryOpening,
                CompleteVictoryPart1,
                CompleteVictoryPart2,
                CompleteVictoryPart3,
                FloydReturns);
        }

        // Ending 2: Stranded - Defense Failed (Course Control fixed, Defense broken)
        if (courseControlFixed && !defenseFixed)
        {
            return string.Join("\n\n",
                CommonOpening,
                VictoryOpening,
                StrandedDefenseFailed);
        }

        // Ending 3: Stranded - Comms Failed (Course Control fixed, Defense fixed, Comms broken)
        if (courseControlFixed && defenseFixed && !commFixed)
        {
            return string.Join("\n\n",
                CommonOpening,
                VictoryOpening,
                StrandedCommsFailed);
        }

        // Ending 4: Doomed Planet with Rescue (Course Control broken, but Comms AND Defense fixed)
        if (!courseControlFixed && commFixed && defenseFixed)
        {
            return string.Join("\n\n",
                CommonOpening,
                DoomedOpening,
                DoomedWithRescue);
        }

        // Ending 5: Doomed Planet, No Rescue (Course Control broken, and either Comms OR Defense broken)
        return string.Join("\n\n",
            CommonOpening,
            DoomedOpening);
    }
}
