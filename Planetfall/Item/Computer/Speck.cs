namespace Planetfall.Item.Computer;

/// <summary>
/// The impurity wedged into the microrelay's contact point — the actual target of the endgame
/// laser puzzle.
///
/// Issue #517: this object was never ported. The relay's description tells the player, in detail,
/// that a speck is what has jammed the relay, and the canonical solution (both the original
/// walkthroughs' and this repo's own walkthrough tests') is "shoot speck with laser" — but with no
/// Speck in the Repository, <c>GetItemInScope("speck")</c> returned null and the laser answered
/// "You don't see any speck here." The only way through the puzzle was to name the *relay*, which
/// on any non-red dial setting shatters it and silently makes the game unwinnable.
///
/// In the original the speck is a first-class object living inside the relay
/// (comptwo.zil:2562-2567) with three synonyms and an adjective, and the laser's SHOOT handler
/// dispatches on it (comptwo.zil:2712 -> SHOOT-SPECK).
///
/// The speck's *state* deliberately stays on <see cref="Relay" /> (SpeckHit / SpeckDestroyed /
/// MarksmanshipCounter) so existing save games and the several consumers of
/// <c>Relay.SpeckDestroyed</c> (hints, Station 384, the microbe) keep working unchanged. This class
/// exists so the noun resolves.
/// </summary>
public class Speck : ItemBase, ICanBeExamined
{
    // SYNONYM SPECK BOULDER IMPURITY / ADJECTIVE BLUE (comptwo.zil:2565-2566).
    public override string[] NounsForMatching =>
        ["speck", "boulder", "impurity", "blue speck", "blue boulder", "blue impurity"];

    // The original gives SPECK no ACTION routine, so examining it falls to the default
    // "nothing special" message. That reads badly here — the engine builds that message from the
    // item's longest noun ("blue impurity"), and the relay has just spent three sentences telling
    // the player exactly what the speck looks like. Reuse the relay's own wording so examining the
    // thing you have been told to shoot actually describes it.
    public string ExaminationDescription =>
        "The speck, presumably of microscopic size, resembles a blue boulder to you in your current size. ";
}
