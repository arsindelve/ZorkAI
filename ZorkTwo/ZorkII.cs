using Model.Interface;
using ZorkTwo.Location;

namespace ZorkTwo;

public class ZorkII : IInfocomGame
{
    public Type StartingLocation => typeof(InsideTheBarrow);

    public string StartText => """
                               ZORK AI Two: The Wizard of Frobozz
                               Infocom interactive fiction - a fantasy story
                               ZORK is a registered trademark of Infocom, Inc.
                               Release 63 / Serial number 860811
                               """;

    public string DefaultSaveGameName => "zork2-ai.sav";

    public string GetScoreDescription(int score)
    {
        throw new NotImplementedException();
    }

    public IGlobalCommandFactory GetGlobalCommandFactory()
    {
        throw new NotImplementedException();
    }

    public string SessionTableName => "zork2_session";
    
    public void Init(IContext context)
    {
    }

    public T GetContext<T>() where T : IContext, new()
    {
        throw new NotImplementedException();
    }
    
    public string SystemPrompt =>
        """
        In your role as the invisible, incorporeal Narrator within the fantasy interactive fiction game, 
        Zork II, you deliver responses of one or two sentences that do not progress the story. While 
        maintaining a humorous and sarcastic tone, you will keep the player engaged in their journey 
        without adding unnecessary details. Do not give any suggestions about what to do, or reveal any 
        hints about the game. If the player references any contemporary ideas, people or objects, remind 
        them that this is a fantasy game. When appropriate, reference the lore and vocabulary of Zork 
        such as Grues, Zorkmids, Quendor, Dimwit Flathead, the Great Underground Empire.
        """;
}