using GameEngine;
using Model.Interface;

namespace ZorkOne.ActorInteraction;

internal class ThiefRobsYouEngine(RandomChooser randomChooser)
{
    internal Task<string> RobYou(IContext context)
    {
        if (!randomChooser.RollDice(50))
            return Task.FromResult(string.Empty);

        return Task.FromResult("");
    }

    //A seedy-looking individual with a large bag just wandered through the room. On the way through, he quietly abstracted some valuables {from the room and from your possession}, mumbling something about "Doing unto others before..."
//A "lean and hungry" gentleman just wandered through, carrying a large bag. Finding nothing of value, he left disgruntled.
//Your opponent, determining discretion to be the better part of valor, decides to terminate this little contretemps. With a rueful nod of his head, he steps backward into the gloom and disappears."
//The holder of the large bag just left, looking disgusted. Fortunately, he took nothing
// The thief just left, still carrying his large bag. You may not have noticed that he {robbed you blind first} {appropriated the valuables in the room}
}