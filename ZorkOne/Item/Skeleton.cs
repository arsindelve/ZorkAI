using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Skeleton : ItemBase
{
    // The original Zork I source declares the skeleton (BONES object) with the synonyms
    // BONES, BODY and SKELETON, so "examine bones"/"examine body" should resolve here.
    public override string[] NounsForMatching =>
        ["skeleton", "bones", "body", "remains", "adventurer", "luckless adventurer"];

    // The classic skeleton curse fires for any of these verbs applied to the bones
    // (zork1/1actions.zil:931 — SKELETON: <VERB? TAKE RUB MOVE PUSH RAISE LOWER ATTACK KICK KISS>).
    // We map each ZIL verb to the engine's thesaurus where one exists (so synonyms like "get"/"kill"/
    // "touch" all trigger the ghost), but deliberately NOT to Verbs.PushVerbs — that family folds in
    // machine-operation synonyms (toggle/flip/switch/activate/click) that read as nonsense on bones.
    // PUSH/RAISE/LOWER/MOVE/KICK/KISS are spelled out with only their sensible "manhandle" synonyms.
    private static readonly string[] CurseVerbs =
        Verbs.TakeVerbs                       // TAKE
            .Concat(Verbs.TouchVerbs)         // RUB
            .Concat(Verbs.KillVerbs)          // ATTACK
            .Concat(["push", "depress", "shove", "move", "raise", "lift", "lower", "kick", "kiss"])
            .ToArray();

    private const string CurseMessage =
        "A ghost appears in the room and is appalled at your desecration of the remains of a fellow " +
        "adventurer. He casts a curse on your valuables and banishes them to the Land of the Living " +
        "Dead. The ghost leaves, muttering obscenities. ";

    // The bones are cursed scenery, not a takeable item; they're listed via GenericDescription
    // (the "fixed item" path in LocationBase.GetItemDescriptions).
    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A skeleton, probably the remains of a luckless adventurer, lies here. ";
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // Only the bones trigger this; non-curse verbs (examine, smell, ...) fall through to the base.
        if (!action.MatchNoun(NounsForMatching) || !action.MatchVerb(CurseVerbs))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        var dead = Repository.GetLocation<LandOfTheDead>();

        // ROB HERE -> LAND-OF-LIVING-DEAD: banish everything else in the room (knife, key, coins,
        // burned-out lantern). Only takeable items, matching ZIL ROB (it moves portable objects, not
        // fixed scenery) — and that filter also excludes the bones themselves, which stay put. ToList()
        // so we don't mutate the collection while iterating (ItemPlacedHere removes from the prior owner).
        foreach (var item in ((ICanContainItems)context.CurrentLocation!).Items
                     .Where(i => i is ICanBeTakenAndDropped).ToList())
            dead.ItemPlacedHere(item);

        // ROB ADVENTURER -> LAND-OF-LIVING-DEAD: empty the player's pack — the lamp goes too, which
        // is exactly why doing this in the dark maze famously strands you.
        foreach (var item in context.Items.ToList())
            dead.ItemPlacedHere(item);

        return new PositiveInteractionResult(CurseMessage);
    }
}
