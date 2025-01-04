using Model.AIGeneration;

namespace Planetfall.Item.Feinstein;

public class Slime : ItemBase, ICanBeExamined, ISmell
{
    public override string[] NounsForMatching => ["slime", "goo", "mess"];

    public override string CannotBeTakenDescription => ExaminationDescription;

    public string ExaminationDescription => "It feels like slime. Aren't you glad you didn't step in it? ";

    public string SmellDescription => "It smells like slime. Aren't you glad you didn't step in it? ";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.MatchVerb(["taste", "eat"]))
            return new PositiveInteractionResult("It tastes like slime. Aren't you glad you didn't step in it? ");

        if (action.MatchVerb(["clean", "scrub", "remove"]))
            return new PositiveInteractionResult("Whew. You've cleaned up maybe one ten-thousandth of the slime. ");

        if (action.MatchVerb(["touch", "feel"]))
            return new PositiveInteractionResult(CannotBeTakenDescription);

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        if (action.Match<Brush>(["clean", "scrub", "remove"], NounsForMatching, ["with", "using"]))
            return new PositiveInteractionResult("Whew. You've cleaned up maybe one ten-thousandth of the slime. ");

        return base.RespondToMultiNounInteraction(action, context);
    }
}