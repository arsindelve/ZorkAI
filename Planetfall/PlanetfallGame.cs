using GameEngine.Location;
using Planetfall.GlobalCommand;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Tower;
using Planetfall.Location.Shuttle;

namespace Planetfall;

public class PlanetfallGame : IInfocomGame
{
    public Type StartingLocation => typeof(DeckNine);

    // The named characters the player can address by name. Declaring them here lets the engine
    // recognize "Floyd, ..." / "Blather, ..." / "ambassador, ..." even when that NPC is not in the
    // room (and even before they have been lazily instantiated), so it can reply "X isn't here."
    // instead of leaking the command into player parsing (see #264).
    public IReadOnlyList<Type> TalkableCharacterTypes =>
        [typeof(Floyd), typeof(Blather), typeof(Ambassador)];

    public string GameName => "Planetfall";

    public string StartText => """
                               Infocom interactive fiction - a science fiction story
                               Copyright (c) 1983, 1988 by Infocom, Inc. All rights reserved.
                               PLANETFALL is a registered trademark of Infocom, Inc.
                               Release 10 / Serial number 880531 / Interpreter 1 Version F

                               Another routine day of drudgery aboard the Stellar Patrol Ship Feinstein. This morning's assignment for a certain lowly Ensign Seventh Class: scrubbing the filthy metal deck at the port end of Level Nine. With your Patrol-issue self-contained multi-purpose all-weather scrub brush you shine the floor with a diligence born of the knowledge that at any moment dreaded Ensign First Class Blather, the bane of your shipboard existence, could appear.

                               """;

    public string DefaultSaveGameName => "planetfall-ai.sav";

    // Planetfall keeps Infocom's grue in-joke for its dark rooms, but with its own wording — "You
    // might be eaten by a grue.", NOT Zork's "You are likely to be eaten by a grue." Verbatim from
    // the original DESCRIBE-ROOM routine (planetfall/verbs.zil).
    public string DarkLocationDescription => "It is pitch black. You might be eaten by a grue. ";

    // https://github.com/the-infocom-files/planetfall/blob/834001e0704ceae3000953a79429ba8ad5216077/verbs.zil#L242

    // [23 hints left.] -> ** 3 points for entering the Escape Pod.
    // [22 hints left.] -> ** 3 points for entering the Crag.
    // [21 hints left.] -> 2 points for turning Floyd on for the first time.
    // [18 hints left.] -> ** 4 points for entering Admin Corridor North.
    // [13 hints left.] -> ** 1 point for taking the kitchen access card.
    // [12 hints left.] -> ** 1 point for taking the shuttle access card.
    // [11 hints left.] -> ** 1 point for taking the upper elevator access card.
    // [19 hints left.] -> ** 4 points for entering Storage West.
    // [17 hints left.] -> ** 4 points for entering the Kitchen.
    // [10 hints left.] -> ** 1 point for taking the lower elevator access card.
    // [16 hints left.] -> 4 points for entering the Tower Core.
    // [7 hints left.] -> 6 points for fixing the communications system.
    // [15 hints left.] -> 4 points for entering the Kalamontee Platform.
    // [14 hints left.] -> 4 points for entering the Lawanda Platform.    
    
    // [20 hints left.] -> 2 points for firing the laser for the first time.
    // [9 hints left.] -> 1 point for taking the miniaturization access card.
    // [8 hints left.] -> 2 points for Floyd's death.
    // [6 hints left.] -> 6 points for fixing the planetary defense system.
    // [5 hints left.] -> 6 points for fixing the course control system.
    // [4 hints left.] -> 4 points for entering the Strip Near Station.
    // [3 hints left.] -> 4 points for entering the Auxiliary Booth.
    // [2 hints left.] -> 8 points for fixing the computer.
    // [1 hint left.] -> 5 points for entering the Cryo-Elevator.

    
    
    
    // [20 hints left.] -> ** Reading the graffiti in the Brig?
    // [19 hints left.] -> Attacking, talking to, or throwing something at Blather?
    // [18 hints left.] -> Attacking or talking to the ambassador?
    // [17 hints left.] -> ** Touching, eating, smelling, or looking at the slime? It (feels/smells/tastes) like slime. Aren't you glad you didn't step in it? (Same "feels like" if you take it)
    // [16 hints left.] -> ** Scrubbing the slime? (Clean or scrub) Whew. You've cleaned up maybe one ten-thousandth of the slime. If you hurry, it might be all cleaned up before Ensign Blather gets here.
    // DONE [15 hints left.] -> ** Eating the celery? // >eat celery Oops. Looks like Blow'k-Bibben-Gordoan metabolism is not compatible with our own. You die of all sorts of convulsions.
    // [14 hints left.] -> ** Examining the games and tapes in the Rec Area?
    // DONE [13 hints left.] -> Looking under the table in the Mess Hall?
    // [12 hints left.] -> Kicking, attacking, RUBBING, or kissing Floyd?
    // [11 hints left.] -> Throwing acid at the mutants?
    // [10 hints left.] -> ** Reading your chronometer?
    // [9 hints left.] -> Taking off your chronometer or pouring acid on it?
    // [8 hints left.] -> Getting into bed in the Infirmary?
    // [7 hints left.] -> Scrubbing yourself?
    // DONE [6 hints left.] -> ** Reading the towel?
    // [5 hints left.] -> Removing your uniform while Blather or Floyd are present?
    // [4 hints left.] -> Destroying the mural?
    // [3 hints left.] -> "Stealing" the lower elevator card from Floyd and then showing it to him?
    // [2 hints left.] -> Giving Floyd the Lazarus breast plate?
    // [1 hint left.] -> Typing ZORK?

    public string GetScoreDescription(int score)
    {
        if (score >= 80) return "Galactic Overlord";
        if (score > 72) return "Cluster Admiral";
        if (score > 64) return "System Captain";
        if (score > 48) return "Planetary Commodore";
        if (score > 36) return "Lieutenant";
        if (score > 24) return "Ensign First Class";
        if (score > 12) return "Space Cadet";

        return "Beginner";
    }

    public IGlobalCommandFactory GetGlobalCommandFactory()
    {
        return new PlanetfallGlobalCommandFactory();
    }

    public string SessionTableName => "planetfall_session";

    public void Init(IContext context)
    {
        // Note: GetLocation already calls Init() internally when creating the location,
        // so we don't call Init() explicitly here to avoid double-initialization.
        // The explicit call was causing BulkheadDoor to be added twice to EscapePod.Items.
        Repository.GetLocation<EscapePod>();
        var explosion = new ExplosionCoordinator();
        context.RegisterActor(Repository.GetLocation<DeckNine>());
        context.RegisterActor(explosion);
    }

    public string SystemPromptSecretKey => "PlanetfallPrompt";

    /// <summary>
    ///     Re-seats the elevator shaft doors, for sessions saved before each room got its own (#532).
    /// </summary>
    /// <remarks>
    ///     Until #532 a shaft had one door object seeded into two rooms, so a blob written by an older
    ///     build has the shared <c>UpperElevatorDoor</c> / <c>LowerElevatorDoor</c> sitting in the
    ///     Elevator Lobby's <c>Items</c>, and nothing at all in the Tower Core or the Waiting Area.
    ///     Restored as-is, the lobby's verbs would find that shared door — <c>LocationBase</c> routes over
    ///     <c>Items</c> without a scope check — and it reports the raw shaft flag, which is only true from
    ///     inside the car. That is #505 all over again, and permanently: the stateless deployment
    ///     rehydrates from the blob every turn, so an in-flight game would never heal itself. Verified by
    ///     restoring a pre-split blob: <c>examine blue door</c> answered "The door is open." on the same
    ///     turn <c>north</c> answered "The door is closed."
    ///     <para>
    ///         Only the three landing rooms are touched. The car keeps the shaft door, which is still
    ///         where the one open/closed flag lives, so no state is migrated — only which object stands in
    ///         which room. Idempotent, so it is harmless on an already-current blob.
    ///     </para>
    /// </remarks>
    public void AfterRestore(IContext context)
    {
        ReseatDoors(Repository.GetLocation<ElevatorLobby>(),
            Repository.GetItem<UpperElevatorLobbyDoor>(), Repository.GetItem<LowerElevatorLobbyDoor>());
        ReseatDoors(Repository.GetLocation<TowerCore>(), Repository.GetItem<UpperElevatorTowerDoor>());
        ReseatDoors(Repository.GetLocation<WaitingArea>(), Repository.GetItem<LowerElevatorWaitingAreaDoor>());
    }

    /// <summary>
    ///     Makes <paramref name="doors" /> exactly the elevator doors standing in <paramref name="room" />,
    ///     evicting any other the restored blob left there.
    /// </summary>
    private static void ReseatDoors(LocationBase room, params ElevatorDoorBase[] doors)
    {
        // An older blob left the shared shaft door here. It belongs to the car now, and its
        // CurrentLocation is owned by whichever room legitimately holds it, so only drop it from this
        // room's contents - never touch the door object itself, and never touch the shaft's flag.
        foreach (var stale in room.Items.OfType<ElevatorDoorBase>().Except(doors).ToList())
            room.RemoveItem(stale);

        foreach (var door in doors.Where(door => !room.Items.Contains(door)))
            room.ItemPlacedHere(door);
    }
}
