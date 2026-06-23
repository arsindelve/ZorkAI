using GameEngine;
using Model;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Command;

/// <summary>
///     The "spirit" command handler — the C# equivalent of the original ZIL DEAD-FUNCTION
///     (zork1/1actions.zil:3113). Once the player has died as a spirit (<see cref="ZorkIContext.IsDead" />),
///     they wander the dungeon as a ghost and almost every verb is overridden with a canned response.
///     The two things a spirit can still do are <b>move</b> (to walk back toward the altar) and
///     <b>pray</b> at the altar (to be resurrected). See issue #17.
/// </summary>
public class SpiritProcessor
{
    /// <summary>
    ///     Handle the raw command for a player who is currently a spirit.
    /// </summary>
    /// <param name="input">The raw, unparsed player input.</param>
    /// <param name="context">The Zork game context (already known to be in the spirit/DEAD state).</param>
    /// <returns>
    ///     The ghost-world response, or <c>null</c> to let the engine process the command normally —
    ///     used for movement so the player can walk back to the altar.
    /// </returns>
    public string? Process(string? input, ZorkIContext context)
    {
        var command = input?.Trim().ToLowerInvariant() ?? string.Empty;

        // Movement still works — a spirit walks the dungeon back to the altar. Returning null lets the
        // engine's normal movement processing run (the player is always-lit while dead, so no grue).
        // The original DEAD-FUNCTION likewise lets WALK fall through (zork1/1actions.zil:3114).
        if (DirectionParser.IsDirection(command, out _))
            return null;

        // The branches below mirror the DEAD-FUNCTION COND, IN ORDER — the first match wins, exactly as
        // in the ZIL. The ordering is load-bearing: "rub"/"touch" must be caught by the OPEN/CLOSE/...
        // bucket (zork1/1actions.zil:3122) before the TAKE/RUB bucket (:3134), so they get "beyond your
        // capabilities", not "your hand passes through". Verb synonyms come from Verbs.cs so the spirit
        // recognizes the same words as the rest of the engine.

        // Praying is how a spirit is resurrected — but only at the altar.
        if (Matches(command, "pray"))
            return Pray(context);

        // LAMP-ON: "turn on lamp" / "light lamp". Resolved before the generic TURN bucket below, just as
        // the ZIL parser distinguishes the LAMP-ON verb from a bare TURN (zork1/1actions.zil:3130).
        if (Matches(command, "turn on", "light", "activate") && MentionsLamp(command))
            return "You need no light to guide you. ";

        // ATTACK, MUNG, ALARM, SWING.
        if (Matches(command, Verbs.KillVerbs) || Matches(command, Verbs.BreakVerbs) || Matches(command, "swing", "alarm"))
            return "All such attacks are vain in your condition. ";

        // OPEN, CLOSE, EAT, DRINK, INFLATE, DEFLATE, TURN, BURN, TIE, UNTIE, RUB (rub/touch live here).
        if (Matches(command, Verbs.OpenVerbs) || Matches(command, Verbs.CloseVerbs) ||
            Matches(command, Verbs.DrinkVerbs) || Matches(command, Verbs.TouchVerbs) ||
            Matches(command, "eat", "inflate", "deflate", "turn", "burn", "tie", "untie"))
            return "Even such an action is beyond your capabilities. ";

        if (Matches(command, "wait", "z"))
            return "Might as well. You've got an eternity. ";

        if (Matches(command, "score"))
            return "You're dead! How can you think of your score? ";

        // TAKE (rub already handled above, so this is take/get/grab/... only).
        if (Matches(command, Verbs.TakeVerbs))
            return "Your hand passes through its object. ";

        // DROP, THROW, INVENTORY.
        if (Matches(command, Verbs.DropVerbs) || Matches(command, Verbs.ThrowVerbs) || Matches(command, "inventory", "i"))
            return "You have no possessions. ";

        if (Matches(command, "diagnose"))
            return "You are dead. ";

        if (Matches(command, Verbs.LookVerbs))
            return LookWhileDead(context);

        return "You can't even do that. ";
    }

    /// <summary>
    ///     Praying at the altar resurrects the spirit: it clears the DEAD state, lifts the player out of
    ///     the underworld and deposits them, unencumbered, back in the forest. Anywhere else, the prayer
    ///     goes unanswered. (ZIL DEAD-FUNCTION PRAY branch, zork1/1actions.zil:3152.)
    /// </summary>
    private static string Pray(ZorkIContext context)
    {
        if (context.CurrentLocation is not Altar)
            return "Your prayers are not heard. ";

        context.IsDead = false;
        var forest = Repository.GetLocation<ForestOne>();
        context.CurrentLocation = forest;

        return "From the distance the sound of a lone trumpet is heard. The world seems to dissolve " +
               "around you, and you feel yourself rising as if from a long sleep, deep in the woods. " +
               "You are unencumbered by any earthly possessions.\n\n" +
               forest.GetDescription(context);
    }

    /// <summary>
    ///     The spirit's LOOK. Matches the ZIL DEAD-FUNCTION LOOK branch (zork1/1actions.zil:3141): the
    ///     "objects appear indistinct" clause is only added when the room actually contains something.
    /// </summary>
    private static string LookWhileDead(ZorkIContext context)
    {
        var roomHasItems = ((ICanContainItems)context.CurrentLocation).Items.Any();
        return roomHasItems
            ? "The room looks strange and unearthly, and objects appear indistinct. "
            : "The room looks strange and unearthly. ";
    }

    private static bool Matches(string command, params string[] verbs)
    {
        // Whole-verb match: the command is exactly the verb, or the verb followed by its object.
        return verbs.Any(v => command == v || command.StartsWith(v + " ", StringComparison.Ordinal));
    }

    private static bool MentionsLamp(string command)
    {
        // Deliberately not matching "light": it is also a turn-on verb (handled by the caller's verb
        // list), so treating it as a lamp noun here would be ambiguous.
        return command.Contains("lamp") || command.Contains("lantern");
    }
}
