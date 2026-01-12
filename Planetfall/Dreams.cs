using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall;

/// <summary>
/// Contains dream sequences shown when the player sleeps.
/// Dreams are randomly selected with a 60% chance, plus a special Floyd dream with 13% chance.
/// </summary>
public static class Dreams
{
    private static readonly string[] DreamSequences =
    [
        "...You find yourself on the bridge of the Feinstein. Ensign Blather is here, as well as Admiral " +
        "Smithers. You are diligently scrubbing the control panel. Blather keeps yelling at you to scrub " +
        "harder. Suddenly you hit the ship's self-destruct switch! Smithers and Blather howl at you as the " +
        "ship begins exploding! You try to run, but your feet seem to be fused to the deck...",

        "...You gulp down the last of your Ramosian Fire Nectar and ask the andro-waiter for another pitcher. " +
        "This pub makes the finest Nectar on all of Ramos Two, and you and your shipmates are having a pretty " +
        "rowdy time. Through the windows of the pub you can see a mighty, ancient castle, shining in the light " +
        "of the three Ramosian moons. The Fire Nectar spreads through your blood and you begin to feel drowsy...",

        "...Strangely, you wake to find yourself back home on Gallium. Even more strangely, you are only eight " +
        "years old again. You are playing with your pet sponge-cat, Swanzo, on the edge of the pond in your " +
        "backyard. Mom is hanging orange towels on the clothesline. Suddenly the school bully jumps out from " +
        "behind a bush, grabs you, and pushes your head under the water. You try to scream, but cannot. You " +
        "feel your life draining away...",

        "...Your vision slowly returns. You are on a wooded cliff overlooking a waterfall. A rainbow spans the " +
        "falls. Blather stands above you, bellowing that the ground is filthy -- scrub harder! You throw your " +
        "brush at Blather, but it passes thru him as though he were a ghost, and sails over the cliff. Blather " +
        "leaps after the valuable piece of Patrol property, and both plummet into the void...",

        "...At last, the Feinstein has arrived at the historic Nebulon system. It's been five months since the " +
        "last shore leave, and you're anxious for Planetfall. You and some other Ensigns Seventh Class enter " +
        "the shuttle for surfaceside. Suddenly, you're alone on the shuttle, and it's tumbling out of control! " +
        "It lands in the ocean and begins sinking! You try to clamber out, but you are stuck in a giant spider " +
        "web. A giant spider crawls closer and closer..."
    ];

    private const string FloydDream =
        "You are in a busy office crowded with people. The only one you recognize is Floyd. He rushes back " +
        "and forth between the desks, carrying papers and delivering coffee. He notices you, and asks how your " +
        "project is coming, and whether you have time to tell him a story. You look into his deep, trusting eyes...";

    /// <summary>
    /// Gets a random dream sequence, or null if no dream occurs.
    /// 13% chance of Floyd dream (if Fork has been touched/Floyd introduced).
    /// 60% chance of random dream.
    /// 27% chance of no dream.
    /// </summary>
    public static string? GetDream(IContext context, Random random)
    {
        // Check for Floyd dream (13% chance if Floyd has been turned on)
        var floyd = Repository.GetItem<Floyd>();
        if (floyd.HasEverBeenOn && random.Next(100) < 13)
        {
            return "\n" + FloydDream;
        }

        // 60% chance of normal dream
        if (random.Next(100) < 60)
        {
            var selectedDream = DreamSequences[random.Next(DreamSequences.Length)];
            return "\n" + selectedDream;
        }

        // 27% chance of no dream
        return null;
    }
}
