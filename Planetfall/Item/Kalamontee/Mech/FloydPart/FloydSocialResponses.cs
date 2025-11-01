namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class FloydSocialResponses(Floyd floyd)
{
    public InteractionResult? HandleSocialInteraction(SimpleIntent action)
    {
        if (!floyd.IsOn)
            return null;

        if (action.Match(["play"], floyd.NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Play);

        if (action.Match(["kiss"], floyd.NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Kiss);

        if (action.Match(["kick"], floyd.NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Kick);

        if (action.Match(Verbs.KillVerbs, floyd.NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Kill);

        return null;
    }
}
