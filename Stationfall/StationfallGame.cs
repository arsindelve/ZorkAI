using Model.Interface;
using Stationfall.GlobalCommand;

namespace Stationfall;

/// <summary>
///     The Stationfall game definition (the sequel to Planetfall). Model everything here on
///     <c>PlanetfallGame</c>: Stationfall is a Planetfall-family game and will reuse its engine
///     subsystems (Floyd, hunger/sleep clocks) as the port fills in.
/// </summary>
public class StationfallGame : IInfocomGame
{
    // Stationfall opens on Deck Twelve of the Stellar Patrol Ship Duffy.
    // (Original starting room: ship.zil DECK-TWELVE, set as HERE by misc.zil's GO routine.)
    public Type StartingLocation => typeof(DeckTwelve);

    public string GameName => "Stationfall";

    // TODO (Phase 3): Floyd returns in Stationfall — add typeof(Floyd) here once ported so
    // "Floyd, ..." is recognized even when he isn't in the room (see PlanetfallGame).
    public IReadOnlyList<Type> TalkableCharacterTypes => [];

    public string StartText => """
                               STATIONFALL
                               Infocom interactive fiction - a science fiction story

                               (Scaffold: opening narration to be authored from the original.)
                               """;

    public string DefaultSaveGameName => "stationfall-ai.sav";

    // Stationfall keeps Infocom's grue in-joke, but with its own station-flavored wording rather than
    // Zork's "eaten by a grue" or Planetfall's "might be eaten by a grue". Verbatim from the original
    // DESCRIBE-ROOM routine (stationfall/verbs.zil:2668-2676). The VACUUM-STORAGE room has a bespoke
    // variant there; when that room is ported it overrides GetDarkDescription for itself.
    public string DarkLocationDescription => "It is pitch black. You hope there are no grues aboard the station. ";

    public string SessionTableName => "stationfall_session";

    // TODO: design decision — Stationfall's own system-prompt secret key.
    public string SystemPromptSecretKey => "StationfallPrompt";

    // Rank ladder verified against the original's TELL-SCORE (verbs.zil:73-88).
    public string GetScoreDescription(int score)
    {
        if (score >= 80) return "Intergalactic Mega-Hero";
        if (score >= 65) return "Interstellar Superstar";
        if (score >= 50) return "Interplanetary Star";
        if (score >= 40) return "International VIP";
        if (score >= 27) return "Footnote in History";
        if (score >= 17) return "One-Day Flash on the Evening News";
        if (score >= 1) return "Rising Young Insignificant Nobody";
        return "Insignificant Nobody";
    }

    public IGlobalCommandFactory GetGlobalCommandFactory()
    {
        return new StationfallGlobalCommandFactory();
    }

    public void Init(IContext context)
    {
        // TODO (Phase 3): register per-turn daemons here — hunger & sleep clocks (reused from
        // Planetfall), the pyramid's robot-evilness takeover, the hull welders, and Floyd. Nothing to
        // register yet for a bootable Deck Twelve.
    }
}
