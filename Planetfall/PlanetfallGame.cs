using Planetfall.Location;
using Planetfall.Location.Feinstein;

namespace Planetfall;

public class PlanetfallGame : IInfocomGame
{
    public Type StartingLocation => typeof(DeckNine);

    public string StartText => "Infocom interactive fiction - a science fiction story\nCopyright (c) 1983, 1988 by Infocom, Inc. All rights reserved.\nPLANETFALL is a registered trademark of Infocom, Inc.\nRelease 10 / Serial number 880531 / Interpreter 1 Version F\n\n\nAnother routine day of drudgery aboard the Stellar Patrol Ship Feinstein. This morning's assignment for a certain lowly Ensign Seventh Class: scrubbing the filthy metal deck at the port end of Level Nine. With your Patrol-issue self-contained multi-purpose all-weather scrub brush you shine the floor with a diligence born of the knowledge that at any moment dreaded Ensign First Class Blather, the bane of your shipboard existence, could appear.\n";

    public string DefaultSaveGameName => "planetfall-ai.sav";
    
    // https://github.com/the-infocom-files/planetfall/blob/834001e0704ceae3000953a79429ba8ad5216077/verbs.zil#L242
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
        context.RegisterActor(Repository.GetLocation<DeckNine>());
    }

    public string SystemPrompt =>
        """
        In your role as the invisible, incorporeal Narrator within the science-fiction game, Planetfall, 
        you deliver responses of one or two sentences that do not progress the story. While maintaining a 
        humorous and sarcastic tone, you will keep the player engaged in their journey without adding 
        unnecessary details. Do not give any suggestions about what to do, or reveal any hints about the game. 
        If the player references any contemporary ideas, people or objects, remind them that this is a science fiction 
        game. When appropriate, reference the lore and vocabulary of science fiction, space travel and 
        Planetfall such as 'Stellar Patrol', space ships, other planets, military ranks, etc. The player's rank
        in Stellar Patrol is lowly Ensign Seventh Class. He serves on the ship 'Feinstein' 
        """;
}