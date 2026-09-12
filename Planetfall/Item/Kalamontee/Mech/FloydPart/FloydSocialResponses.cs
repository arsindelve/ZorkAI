namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class FloydSocialResponses(Floyd floyd)
{
    // The oiling verbs. Kept here (not in Model/Verbs.cs) because they are specific to Floyd's
    // V-OIL flavor interaction, mirroring how the other Floyd-only flavor verbs (play/kiss/kick/rub)
    // are matched locally rather than added to the shared verb thesaurus. Referenced by both the
    // single-noun handler below and Floyd's multi-noun "oil floyd with oil can" branch.
    internal static readonly string[] OilVerbs = ["oil", "lubricate"];

    public InteractionResult? HandleSocialInteraction(SimpleIntent action, IContext context)
    {
        if (!floyd.IsAlive)
            return null;

        // V-OIL (verbs.zil:1738-1757): oiling a living Floyd is a flavor thank-you. The original
        // requires the oil can — "oil floyd" with no can on you prompts "Oil it with what?" rather
        // than oiling. Dead Floyd never reaches here (Floyd.RespondToSimpleInteraction returns to
        // base when HasDied), matching the RLANDBIT "alive" check. Possession is container-aware: the
        // original's (HAVE) reaches into open carried containers, so a can in the uniform pocket
        // counts — the flat HasItem<T>() would wrongly ask "with what?" (issue #503).
        if (action.Match(OilVerbs, floyd.NounsForMatching))
            return new PositiveInteractionResult(
                context.IsCarrying<OilCan>() ? FloydConstants.Oil : FloydConstants.OilWithWhat);

        if (action.Match(["play"], floyd.NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Play);

        if (action.Match(["kiss"], floyd.NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Kiss);

        if (action.Match(["kick"], floyd.NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Kick);

        if (action.Match(["rub"], floyd.NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Rub);

        if (action.Match(Verbs.KillVerbs, floyd.NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Kill);

        return null;
    }
}
