using GameEngine;
using Model.Interface;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Command;

/// <summary>
///     Class responsible for processing player death — the C# equivalent of the original ZIL
///     JIGS-UP routine (zork1/1actions.zil). The outcome depends on how many times the player has
///     died and whether they have ever visited the altar:
///     <list type="bullet">
///         <item>Killed while already a spirit, or a third death, is a permanent game over.</item>
///         <item>
///             Otherwise, if the player has visited the altar they die "as a spirit" at the Entrance
///             to Hades; if not, they reincarnate in the forest as before.
///         </item>
///         <item>Either reincarnation costs 10 points and scatters the player's inventory.</item>
///     </list>
///     See issue #17.
/// </summary>
public class DeathProcessor
{
    /// <summary>
    ///     The above-ground rooms across which a dead player's possessions are scattered. This mirrors
    ///     the ZIL ABOVE-GROUND table exactly (zork1/1dungeon.zil:2630) — the eleven rooms RANDOMIZE-OBJECTS
    ///     uses for everything that isn't the lamp or the coffin. Note the C# names: BehindHouse is the
    ///     ZIL EAST-OF-HOUSE ("Behind House"); Clearing is GRATING-CLEARING (the one with the grating) and
    ///     ClearingBehindHouse is the plain CLEARING. ForestFour is deliberately absent — it is not in the
    ///     original table.
    /// </summary>
    /// <remarks>
    ///     Simplification: the original scatters non-lamp/coffin <em>treasures</em> to random dark land
    ///     rooms and only non-treasures to ABOVE-GROUND (zork1/1actions.zil:4109). We send every remaining
    ///     item to ABOVE-GROUND, which keeps the player's stuff recoverable without a treasure-value model.
    /// </remarks>
    private static List<ILocation> AboveGroundRooms =>
    [
        Repository.GetLocation<WestOfHouse>(), Repository.GetLocation<NorthOfHouse>(),
        Repository.GetLocation<BehindHouse>(), Repository.GetLocation<SouthOfHouse>(),
        Repository.GetLocation<ForestOne>(), Repository.GetLocation<ForestTwo>(),
        Repository.GetLocation<ForestThree>(), Repository.GetLocation<ForestPath>(),
        Repository.GetLocation<ClearingBehindHouse>(), Repository.GetLocation<Clearing>(),
        Repository.GetLocation<CanyonView>()
    ];

    /// <summary>
    ///     Process the death of the player. Oh, no!
    /// </summary>
    /// <param name="death">The death message.</param>
    /// <param name="context">The current game context.</param>
    /// <returns>Returns an instance of InteractionResult.</returns>
    /// <exception cref="ArgumentException">Thrown if the context is not of type ZorkIContext.</exception>
    public InteractionResult Process(string death, IContext context)
    {
        if (context is not ZorkIContext zorkContext)
            throw new ArgumentException("DeathProcessor requires a ZorkIContext.", nameof(context));

        // It takes a talented person to be killed while already dead: a spirit's death is permanent.
        // HasPermanentlyDied ends the game (the engine has no FINISH), so the spirit can no longer
        // wander back to the altar and pray its way out.
        if (zorkContext.IsDead)
        {
            zorkContext.HasPermanentlyDied = true;
            return new PositiveInteractionResult(death +
                "\n\nIt takes a talented person to be killed while already dead. I think you deserve a " +
                "permanent rest.\n\n\t*** You have died ***\n");
        }

        zorkContext.LightWoundCounter = 0;
        zorkContext.AddPoints(-10);

        // Two reincarnations are all you get; the third death is the end of the line.
        if (zorkContext.DeathCounter >= 2)
        {
            zorkContext.DeathCounter++;
            zorkContext.HasPermanentlyDied = true;
            return new PositiveInteractionResult(death +
                "\n\n\t*** You have died ***\n\n" +
                "You clearly are a suicidal maniac. We don't allow your type around here. You'll be a " +
                "permanent resident of the Land of the Living Dead!\n");
        }

        zorkContext.DeathCounter++;
        ScatterInventory(zorkContext);

        // Once you've visited the altar, death makes you a spirit at the gates of Hell rather than
        // granting a fresh start in the forest.
        if (zorkContext.HasVisitedAltar)
        {
            zorkContext.IsDead = true;
            var hades = Repository.GetLocation<EntranceToHades>();
            context.CurrentLocation = hades;

            return new PositiveInteractionResult(death +
                "\n\nAs you take your last breath, you feel relieved of your burdens. The feeling passes " +
                "as you find yourself before the gates of Hell, where the spirits jeer at you and deny you " +
                "entry. Your senses are disturbed. The objects in the dungeon appear indistinct, bleached " +
                "of color, even unreal.\n\n" +
                hades.GetDescription(context));
        }

        // The familiar reincarnation: dumped back in the forest with another chance.
        var forest = Repository.GetLocation<ForestOne>();
        context.CurrentLocation = forest;

        return new PositiveInteractionResult(death +
            "\n\t*** You have died ***\n\n" +
            "Now, let's take a look here... Well, you probably deserve another chance. I can't quite fix " +
            "you up completely, but you can't have everything.\n\n" +
            forest.GetDescription(context));
    }

    /// <summary>
    ///     Scatters the player's possessions across the dungeon (ZIL RANDOMIZE-OBJECTS). The lamp goes
    ///     to the Living Room and the coffin to the Egyptian Room; everything else lands in a random
    ///     above-ground room. Uses the context's <see cref="IRandomChooser" /> so it is testable.
    /// </summary>
    private static void ScatterInventory(ZorkIContext context)
    {
        foreach (var item in context.Items.ToList())
        {
            var destination = item switch
            {
                Lantern => Repository.GetLocation<LivingRoom>(),
                Coffin => Repository.GetLocation<EgyptianRoom>(),
                _ => context.Chooser.Choose(AboveGroundRooms)
            };

            // Take it out of the player's hands explicitly before placing it. ItemPlacedHere removes
            // an item from its previous owner, but resolving a not-yet-visited destination room can
            // lazily run that room's Init (which re-places its own starting items, e.g. the lamp in
            // the Living Room) without clearing the inventory copy — so we drop it ourselves first.
            context.RemoveItem(item);
            destination.ItemPlacedHere(item);
        }
    }
}
