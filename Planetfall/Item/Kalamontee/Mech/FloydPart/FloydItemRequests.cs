using System.Text.RegularExpressions;
using GameEngine;

namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

/// <summary>
///     Asking Floyd to hand back, or to put down, the thing he is holding. Both are real mechanics in
///     the original — <c>V?ASK-FOR</c> (<c>compone.zil:2053-2060</c>) and <c>V?DROP</c>
///     (<c>compone.zil:1912-1923</c>) in the "Floyd is the WINNER" half of <c>FLOYD-F</c> — but the
///     port only ever routed them to the conversation service, which is instructed to decline every
///     physical request in character. So Floyd confidently claimed he "cannot drop things" while
///     <c>take &lt;item&gt;</c> retrieved it from him on the very next turn (issue #521).
///     <para>
///     This is a DETERMINISTIC, pre-LLM bridge on purpose. Metadata was the other option, but the
///     companion Lambda emits no intent for these requests (its prompt classifies give/drop as
///     "just declines"), and the self-hosted path returns no metadata at all
///     (<see cref="Planetfall.AI.LocalCompanionChat" />), so a metadata bridge would fix neither
///     backend today. Matching the player's own words costs no round-trip and works in both.
///     </para>
///     <para>
///     Everything unrecognized returns null and falls through to the conversation service exactly as
///     before, so this can only ever add behavior. That fall-through is load-bearing, not laziness:
///     "floyd, give me a hug" must stay a joke, not become a handover.
///     </para>
/// </summary>
public class FloydItemRequests(Floyd floyd)
{
    /// <summary>
    ///     The "hand it over" verbs, with their -ing forms, because a spoken request is free-form prose
    ///     rather than a parsed verb ("would you mind GIVING me the diary"). Matched as whole words.
    /// </summary>
    private static readonly string[] HandBackVerbs =
        ["give", "giving", "hand", "handing", "pass", "passing", "return", "returning", "gimme"];

    /// <summary>
    ///     The recipient must be the PLAYER. The original's give branch fires only on
    ///     <c>&lt;EQUAL? ,PRSI ,ME&gt;</c> (<c>compone.zil:1855-1859</c>), so "floyd, give the diary to
    ///     the ambassador" is not this mechanic and stays Floyd's own business.
    /// </summary>
    private static readonly string[] RecipientIsThePlayer = ["me", "myself", "back"];

    /// <summary>
    ///     Polite requests that carry no give verb at all. These are the phrasings a player reaches for
    ///     most often, and the original's own grammar for them is ASK ... FOR (<c>syntax.zil:341-342</c>).
    /// </summary>
    private static readonly string[] HandBackIdioms =
        ["can i have", "could i have", "may i have", "let me have", "hand over", "i want", "i'd like"];

    /// <summary>
    ///     The "put it down" verbs. "leave" is deliberately absent: "floyd, leave it alone" is a
    ///     forbidding, not a request to drop (the negation guard below would catch that phrasing, but
    ///     the verb list should not be reaching for it in the first place).
    /// </summary>
    private static readonly string[] DropVerbs =
        ["drop", "dropping", "put down", "putting down", "set down", "let go of", "release"];

    /// <summary>
    ///     Words that make the held item the object without naming it ("give it back", "drop that").
    ///     Only consulted when Floyd actually has something in his hand, which is the only thing they
    ///     could be pointing at.
    /// </summary>
    private static readonly string[] PronounsForTheHeldItem = ["it", "that", "this", "them", "those"];

    /// <summary>
    ///     Words that may follow the pronoun while it is still the OBJECT of the request ("give it
    ///     back", "give it to me", "drop it now"). Anything else following one means it was a
    ///     determiner in front of a real noun instead — "give me that joke", "tell me this story" —
    ///     which is not a request for the thing in his hand and must keep reaching the chat service.
    /// </summary>
    private static readonly string[] PronounTails = ["back", "down", "to", "over", "please", "now", "here"];

    /// <summary>
    ///     A negated or forbidding request is never an action — "don't drop the diary" is the opposite
    ///     of a drop. Cheap to check and expensive to get wrong, so it gates both mechanics.
    /// </summary>
    private static readonly string[] Negations =
        ["don't", "do not", "dont", "never", "stop", "please don't", "no need", "instead of"];

    /// <summary>
    ///     Bridges a spoken request to the mechanic behind it. Returns null — the common case — when the
    ///     utterance is not a request for the thing in Floyd's hand, leaving it for the conversation
    ///     service.
    /// </summary>
    internal string? HandleSpokenRequest(string text, IContext context)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Sentence punctuation becomes whitespace so it cannot weld itself to a word the tests below
        // are looking for - without this, "give me the diary, please" carries "diary," and matches no
        // noun at all. Hyphens and apostrophes survive, because item nouns and contractions need them
        // ("multi-purpose scrub brush", "don't"). Then pad, so every containment test is a whole-word
        // one: " it " must not fire inside "wait".
        var said = $" {Regex.Replace(text.ToLowerInvariant(), "[.,!?;:\"]+", " ").Trim()} ";
        said = Regex.Replace(said, @"\s+", " ");

        if (Negations.Any(n => said.Contains($" {n} ")))
            return null;

        var wantsItBack = (HandBackVerbs.Any(v => said.Contains($" {v} ")) &&
                           RecipientIsThePlayer.Any(r => said.Contains($" {r} "))) ||
                          HandBackIdioms.Any(i => said.Contains($" {i} "));

        var wantsItDropped = DropVerbs.Any(v => said.Contains($" {v} "));

        if (!wantsItBack && !wantsItDropped)
            return null;

        var requested = ResolveRequestedItem(said, context);
        if (requested is null)
            return null;

        return Speak(requested == floyd.ItemBeingHeld
            ? wantsItBack ? HandOver(requested, context) : LetGoOf(requested, context)
            : FloydConstants.DoesNotHaveThat, context);
    }

    /// <summary>
    ///     The original's own command grammar for this: <c>TELL/ASK &lt;actor&gt; FOR &lt;object&gt;</c>
    ///     (<c>syntax.zil:341-342</c>, routed to <c>V-ASK-FOR</c>). It reaches Floyd as an ordinary
    ///     multi-noun command rather than as speech, because the utterance leads with the verb instead
    ///     of his name and so is never detected as direct address.
    ///     <para>
    ///     The player named an object outright here, so a real object Floyd is not holding gets the
    ///     original's flat <c>FLOYD-NOT-HAVE</c> (<c>compone.zil:2059-2060</c>) instead of the spoken
    ///     path's silent fall-through. A noun that resolves to no object at all still falls through.
    ///     </para>
    /// </summary>
    internal InteractionResult? HandleAskFor(MultiNounIntent action, IContext context)
    {
        if (!action.MatchVerb(Verbs.AskForVerbs) || !action.MatchNounOne(floyd.NounsForMatching) ||
            !action.MatchPreposition(["for"]))
            return null;

        var held = floyd.ItemBeingHeld;

        if (held is not null && NounMatch.CouldName(held, action.NounTwo))
            return new PositiveInteractionResult(Speak(HandOver(held, context), context));

        // Named something real that simply isn't in his hand: the original's flat refusal. A noun we
        // cannot resolve at all ("ask floyd for help") is not an object and so not this mechanic - fall
        // through and let the engine answer an unresolvable noun the way it answers any other, rather
        // than have Floyd deny owning a thing that was never a thing.
        if (Repository.GetItemInScope(action.NounTwo, context) is not null)
            return new PositiveInteractionResult(Speak(FloydConstants.DoesNotHaveThat, context));

        return null;
    }

    /// <summary>
    ///     Which item is this request about? In order: the thing in Floyd's hand if it is named or
    ///     pointed at, then anything else in scope the player named (which earns the original's "Floyd
    ///     does not one of those have!"), and otherwise nothing at all — a request that names no object
    ///     is not this mechanic.
    ///     <para>
    ///     Deliberately confined to <see cref="Model.Interface.ICanHoldItems.ItemBeingHeld" /> — what is
    ///     literally in his hand — and never his compartments. The lower elevator access card lives in
    ///     those until he reveals it, and in the original it is not inside him at all until the reveal
    ///     daemon moves it there (<c>globals.zil:1456-1463</c>), so ASK-FOR could not produce it early.
    ///     Reaching in here would hand the player a puzzle's answer on turn one.
    ///     </para>
    /// </summary>
    private IItem? ResolveRequestedItem(string said, IContext context)
    {
        var held = floyd.ItemBeingHeld;

        // NounMatch.CouldName, not HasMatchingNoun: the container-derived items override the latter
        // into exact equality, which no free-form sentence will ever satisfy.
        if (held is not null && (NounMatch.CouldName(held, said) || PointsAtItWithoutNaming(said)))
            return held;

        // Candidate order mirrors Repository.GetPreciseMatchInScope: room before inventory.
        var candidates = new List<IItem>();
        if (context.CurrentLocation is ICanContainItems here)
            candidates.AddRange(here.GetAllItemsRecursively ?? []);
        candidates.AddRange(context.GetAllItemsRecursively ?? []);

        return candidates.FirstOrDefault(candidate => NounMatch.CouldName(candidate, said));
    }

    /// <summary>
    ///     Does the request point at the held item with a bare pronoun rather than naming it? True only
    ///     when the pronoun is the OBJECT of the request — the last word, or followed by one of the
    ///     tails that can trail it. The distinction is the whole point: "give it back" and "give it to
    ///     me" are this mechanic, while "give me that joke" and "tell me this story" use the same words
    ///     as determiners in front of something that is not an object at all, and handing over the
    ///     diary because the player asked for a joke would be worse than the bug being fixed.
    /// </summary>
    private static bool PointsAtItWithoutNaming(string said)
    {
        var words = said.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < words.Length; i++)
        {
            if (!PronounsForTheHeldItem.Contains(words[i]))
                continue;

            if (i == words.Length - 1 || PronounTails.Contains(words[i + 1]))
                return true;
        }

        return false;
    }

    /// <summary>
    ///     V-ASK-FOR's success branch (<c>compone.zil:2054-2058</c>): the item goes straight into the
    ///     player's hands, by the same <c>Context.Take</c> the direct "take &lt;item&gt;" command uses.
    /// </summary>
    private string HandOver(IItem item, IContext context)
    {
        ReleaseFromHisHand(item);
        context.Take(item);

        return string.Format(FloydConstants.HandsYouTheItem, item.Name);
    }

    /// <summary>
    ///     V?DROP's success branch (<c>compone.zil:1913-1917</c>): the item lands on the floor.
    ///     <para>
    ///     Deliberate divergence: the original rolls <c>PROB 50</c> and half the time has Floyd clutch
    ///     the object and refuse ("Floyd won't," he says defiantly). He always complies here — owner's
    ///     call. A coin-flip refusal reads as the same stonewalling this issue is about to a player who
    ///     has no way of knowing that asking twice would work.
    ///     </para>
    /// </summary>
    private string LetGoOf(IItem item, IContext context)
    {
        ReleaseFromHisHand(item);
        context.CurrentLocation.ItemPlacedHere(item);

        return string.Format(FloydConstants.ShrugsAndDrops, item.Name);
    }

    /// <summary>
    ///     Takes the item out of Floyd's hand, and retires the take-callback stamp that put it there.
    ///     <para>
    ///     The stamp matters. <see cref="FloydInventoryManager.OfferItem" /> writes
    ///     "Floyd,BeingTakenCallback" onto the item and nothing ever clears it, and that callback nulls
    ///     <c>ItemBeingHeld</c> whoever picks the item up and whenever. An item Floyd once held, left
    ///     lying on the floor, would therefore strip him of whatever he is holding LATER when the player
    ///     picks it up — orphaning that second item with its CurrentLocation still pointing at Floyd but
    ///     in neither his hand nor his compartments, which is out of scope for good. Clearing his grip
    ///     here rather than leaning on the callback keeps that stamp from outliving its one use.
    ///     </para>
    /// </summary>
    private void ReleaseFromHisHand(IItem item)
    {
        floyd.ItemBeingHeld = null;
        item.OnBeingTakenCallback = null;
    }

    /// <summary>
    ///     Finishes a bridged turn: remembers what Floyd said, so his idle chatter can refer back to it,
    ///     and stops him acting again this turn. The original sets <c>FLOYD-SPOKE</c> for every command
    ///     addressed to him (<c>compone.zil:1854</c>) for exactly that reason, and the Repair Room
    ///     bridges already do the same.
    /// </summary>
    private string Speak(string line, IContext context)
    {
        floyd.LastTurnsOutput.Push(line);
        floyd.SkipActingThisTurn(context);
        return line;
    }
}
