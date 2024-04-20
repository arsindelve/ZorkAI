using Model.AIGeneration;
using Model.Intent;

namespace ZorkOne.Item;

public class BrassBell : ItemBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["bell", "brass bell"];

    public override string InInventoryDescription => "A brass bell";

    public string ExaminationDescription => "There is nothing special about the brass bell";

    public string OnTheGroundDescription => "There is a brass bell here.";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client)
    {
        if (action.Verb.ToLowerInvariant() != "ring")
            return base.RespondToSimpleInteraction(action, context, client);
        
        if (!action.MatchNoun(["bell"]))
            return base.RespondToSimpleInteraction(action, context, client);
    
        return new PositiveInteractionResult("Ding, dong");
    }
}