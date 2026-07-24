using Model.Interface;
using Stationfall.Location;

namespace Stationfall;

/// <summary>
///     Scaffold for the Stationfall port (the sequel to Planetfall). This is an intentionally-empty
///     shell that satisfies <see cref="IInfocomGame" /> so the project builds and can be wired into
///     the engine. The real content — rooms, items, Floyd's return, scoring, global commands — is
///     still to be authored. Model everything here on <c>PlanetfallGame</c>: Stationfall is a
///     Planetfall-family game and reuses its engine subsystems (Floyd, hunger/sleep clocks).
/// </summary>
public class StationfallGame : IInfocomGame
{
    // Stationfall opens on Deck Twelve of the Stellar Patrol Ship Duffy.
    // (Original starting room: ship.zil DECK-TWELVE, set as HERE by misc.zil's GO routine.)
    public Type StartingLocation => typeof(DeckTwelve);

    public string GameName => "Stationfall";

    // TODO: Floyd returns in Stationfall. List the talkable NPC types here (e.g. typeof(Floyd)) once
    // ported, so "Floyd, ..." is recognized even when he isn't in the room (see PlanetfallGame).
    public IReadOnlyList<Type> TalkableCharacterTypes => [];

    public string StartText => """
                               STATIONFALL
                               Infocom interactive fiction - a science fiction story

                               (Scaffold: opening narration to be authored from the original.)
                               """;

    public string DefaultSaveGameName => "stationfall-ai.sav";

    public string SessionTableName => "stationfall_session";

    // TODO: design decision — Stationfall's own system-prompt secret key.
    public string SystemPromptSecretKey => "StationfallPrompt";

    // TODO: author the real rank bands (see PlanetfallGame.GetScoreDescription for the pattern).
    public string GetScoreDescription(int score)
    {
        throw new NotImplementedException("Stationfall score ranks not yet authored.");
    }

    // TODO: build a StationfallGlobalCommandFactory (model on PlanetfallGlobalCommandFactory).
    public IGlobalCommandFactory GetGlobalCommandFactory()
    {
        throw new NotImplementedException("Stationfall global command factory not yet built.");
    }

    public void Init(IContext context)
    {
        // TODO: register per-turn actors/daemons here — hunger & sleep clocks (reused from
        // Planetfall), the station's wandering NPCs, and Floyd once he is ported.
    }
}
