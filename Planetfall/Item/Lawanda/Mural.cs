using Model.AIGeneration;
using Model.Interface;

namespace Planetfall.Item.Lawanda;

public class Mural : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["mural"];

    public override string CannotBeTakenDescription => "The mural is firmly attached to the wall. ";

    public string ExaminationDescription =>
        "It's a gaudy work of orange and purple abstract shapes, reminiscent of the early works " +
        "of Burstini Bonz. It doesn't appear to fit the decor of the room at all. The mural seems to ripple " +
        "now and then, as though a breeze were blowing behind it. ";

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.KillVerbs, NounsForMatching))
        {
            return new PositiveInteractionResult("My sentiments also, but let's be civil.");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
