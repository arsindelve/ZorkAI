using Model.AIGeneration;

namespace Planetfall.Item.Feinstein;

public class Slime : ItemBase, ICanBeExamined, ISmell
{
    public override string[] NounsForMatching => ["slime", "goo", "mess"];

    public override string CannotBeTakenDescription => ExaminationDescription;

    public string ExaminationDescription => "It feels like slime. Aren't you glad you didn't step in it? ";

    public string SmellDescription => "It smells like slime. Aren't you glad you didn't step in it? ";

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // Must check both verb AND noun to avoid intercepting commands like "remove battery"
        if (action.Match(Verbs.TasteVerbs, NounsForMatching))
            return new PositiveInteractionResult("It tastes like slime. Aren't you glad you didn't step in it? ");

        if (action.Match(Verbs.CleanOffVerbs, NounsForMatching))
            return new PositiveInteractionResult("Whew. You've cleaned up maybe one ten-thousandth of the slime. ");

        if (action.Match(Verbs.TouchVerbs, NounsForMatching))
            return new PositiveInteractionResult(CannotBeTakenDescription);

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        if (action.Match<Brush>(Verbs.CleanOffVerbs, NounsForMatching, ["with", "using"]))
            return new PositiveInteractionResult("Whew. You've cleaned up maybe one ten-thousandth of the slime. ");

        return await base.RespondToMultiNounInteraction(action, context);
    }
}