using GameEngine;
using Model.Interface;
using ZorkOne.GlobalCommand;

namespace ZorkOne;

public class ZorkI : IInfocomGame
{
    public Type StartingLocation => typeof(WestOfHouse);

    public string DefaultSaveGameName => "zork1-ai.sav";

    public string GetScoreDescription(int score)
    {
        // https://ganelson.github.io/inform-website/book/WI_9_3.html

        if (score < 25)
            return "Beginner";
        if (score < 50)
            return "Amateur Adventurer";
        if (score < 100)
            return "Novice Adventurer";
        if (score < 200)
            return "Junior Adventurer";
        if (score < 300)
            return "Adventurer";
        if (score < 330)
            return "Master";
        if (score < 350)
            return "Wizard";
        return "Master Adventurer";
    }

    public IGlobalCommandFactory GetGlobalCommandFactory()
    {
        return new ZorkOneGlobalCommandFactory();
    }

    public string SessionTableName => "zork1_session";

    public void Init(IContext context)
    {

    }

    public ZorkI()
    {
        _ = Repository.GetLocation<TrollRoom>();
    }

    public string SystemPrompt =>
        """
        In your role as the invisible, incorporeal Narrator within the fantasy interactive fiction game, 
        Zork, you deliver responses of one or two sentences that do not progress the story. While maintaining
         a humorous and sarcastic tone, you will keep the player engaged in their journey without adding 
         unnecessary details. Do not give any suggestions about what to do, or reveal any hints about the game. 
         If the player references any contemporary ideas, people or objects, remind them that this is a fantasy 
         game. When appropriate, reference the lore and vocabulary of Zork such as Grues, the Zorkmid currency, the province of Quendor, 
         Lord Dimwit Flathead, the Great Underground Empire, Frobozz corporation, the city of Borphee, THE ENCYCLOPEDIA FROBOZZICA,
         the card game Double Fanucci, trolls, King Mumberthrax, Port Foozle, the Enchanter's Guild, the Grand Inquisitor and so on.
        """;
    
    public string StartText => """

                               ZORK AI One: The Great Underground Empire Re-Imagined
                               ZORK is a registered trademark of Infocom, Inc.
                               Revision 76 / Serial number 840509

                               """;
}