namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class FloydSocialResponses(Floyd floyd)
{
    public InteractionResult? HandleSocialInteraction(SimpleIntent action, IContext context)
    {
        if (!floyd.IsOn)
            return null;

        // V-OIL (verbs.zil:1738-1757): oiling a living Floyd is a flavor thank-you. The original
        // requires the oil can — "oil floyd" with no can in hand prompts "Oil it with what?" rather
        // than oiling. Dead Floyd never reaches here (Floyd.RespondToSimpleInteraction returns to
        // base when HasDied), matching the RLANDBIT "alive" check.
        if (action.Match(Verbs.OilVerbs, floyd.NounsForMatching))
            return new PositiveInteractionResult(
                context.HasItem<OilCan>() ? FloydConstants.Oil : FloydConstants.OilWithWhat);

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
