namespace Planetfall;

public class PlanetfallGame : IInfocomGame
{
    public Type StartingLocation => typeof(DeckNine);

    public string StartText => """
                               Infocom interactive fiction - a science fiction story
                               Copyright (c) 1983, 1988 by Infocom, Inc. All rights reserved.
                               PLANETFALL is a registered trademark of Infocom, Inc.
                               Release 10 / Serial number 880531 / Interpreter 1 Version F

                               Another routine day of drudgery aboard the Stellar Patrol Ship Feinstein. This morning's assignment for a certain lowly Ensign Seventh Class: scrubbing the filthy metal deck at the port end of Level Nine. With your Patrol-issue self-contained multi-purpose all-weather scrub brush you shine the floor with a diligence born of the knowledge that at any moment dreaded Ensign First Class Blather, the bane of your shipboard existence, could appear.

                               """;

    public string DefaultSaveGameName => "planetfall-ai.sav";
    
    // https://github.com/the-infocom-files/planetfall/blob/834001e0704ceae3000953a79429ba8ad5216077/verbs.zil#L242
    
    // [23 hints left.] -> 3 points for entering the Escape Pod.
    // [22 hints left.] -> 3 points for entering the Crag.
    // [21 hints left.] -> 2 points for turning Floyd on for the first time.
    // [20 hints left.] -> 2 points for firing the laser for the first time.
    // [19 hints left.] -> 4 points for entering Storage West.
    // [18 hints left.] -> 4 points for entering Admin Corridor North.
    // [17 hints left.] -> 4 points for entering the Kitchen.
    // [16 hints left.] -> 4 points for entering the Tower Core.
    // [15 hints left.] -> 4 points for entering the Kalamontee Platform.
    // [14 hints left.] -> 4 points for entering the Lawanda Platform.
    // [13 hints left.] -> 1 point for taking the kitchen access card.
    // [12 hints left.] -> 1 point for taking the shuttle access card.
    // [11 hints left.] -> 1 point for taking the upper elevator access card.
    // [10 hints left.] -> 1 point for taking the lower elevator access card.
    // [9 hints left.] -> 1 point for taking the miniaturization access card.
    // [8 hints left.] -> 2 points for Floyd's death.
    // [7 hints left.] -> 6 points for fixing the communications system.
    // [6 hints left.] -> 6 points for fixing the planetary defense system.
    // [5 hints left.] -> 6 points for fixing the course control system.
    // [4 hints left.] -> 4 points for entering the Strip Near Station.
    // [3 hints left.] -> 4 points for entering the Auxiliary Booth.
    // [2 hints left.] -> 8 points for fixing the computer.
    // [1 hint left.] -> 5 points for entering the Cryo-Elevator.
    
    // [20 hints left.] -> Reading the graffiti in the Brig?
    // [19 hints left.] -> Attacking, talking to, or throwing something at Blather?
    // [18 hints left.] -> Attacking or talking to the ambassador?
    // [17 hints left.] -> Touching, eating, smelling, or looking at the slime? It (feels/smells/tastes) like slime. Aren't you glad you didn't step in it? (Same "feels like" if you take it)
    // [16 hints left.] -> Scrubbing the slime? (Clean or scrub) Whew. You've cleaned up maybe one ten-thousandth of the slime. If you hurry, it might be all cleaned up before Ensign Blather gets here.
    // [15 hints left.] -> Eating the celery? // >eat celery Oops. Looks like Blow'k-Bibben-Gordoan metabolism is not compatible with our own. You die of all sorts of convulsions.
    // [14 hints left.] -> Examining the games and tapes in the Rec Area?
    // [13 hints left.] -> Looking under the table in the Mess Hall?
    // [12 hints left.] -> Kicking, attacking, rubbing, or kissing Floyd?
    // [11 hints left.] -> Throwing acid at the mutants?
    // [10 hints left.] -> Reading your chronometer?
    // [9 hints left.] -> Taking off your chronometer or pouring acid on it?
    // [8 hints left.] -> Getting into bed in the Infirmary?
    // [7 hints left.] -> Scrubbing yourself?
    // [6 hints left.] -> Reading the towel?
    // [5 hints left.] -> Removing your uniform while Blather or Floyd are present?
    // [4 hints left.] -> Destroying the mural?
    // [3 hints left.] -> "Stealing" the lower elevator card from Floyd and then showing it to him?
    // [2 hints left.] -> Giving Floyd the Lazarus breast plate?
    // [1 hint left.] -> Typing ZORK?
    
    public string GetScoreDescription(int score)
    {
        if (score >= 80)
        {
            return "Galactic Overlord";
        }
        if (score > 72)
        {
            return "Cluster Admiral";
        }
        if (score > 64)
        {
            return "System Captain";
        }
        if (score > 48)
        {
            return "Planetary Commodore";
        }
        if (score > 36)
        {
            return "Lieutenant";
        }
        if (score > 24)
        {
            return "Ensign First Class";
        }
        if (score > 12)
        {
            return "Space Cadet";
        }
    
        return "Beginner";
    }

    public IGlobalCommandFactory GetGlobalCommandFactory()
    {
        return new PlanetfallGlobalCommandFactory();
    }

    public string SessionTableName => "planetfall_session";
    
    public void Init(IContext context)
    {
        Repository.GetLocation<EscapePod>().Init();
        var explosion = new ExplosionCoordinator();
        context.RegisterActor(Repository.GetLocation<DeckNine>());
        context.RegisterActor(explosion);
    }
    
    public string SystemPromptSecretKey => "PlanetfallPrompt";
}