using Model.AIGeneration;
using Planetfall.Command;

namespace Planetfall.Item.Kalamontee.Mech;

public class Flask : ItemBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["glass flask", "flask"];

    public string? LiquidColor { get; set; }

    public string ExaminationDescription =>
        "The flask has a wide mouth and looks large enough to hold one or two liters. It is made of glass, " +
        "or perhaps some tough plastic " +
        (string.IsNullOrEmpty(LiquidColor) ? "." : "and is filled with a milky white fluid. ");

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Sitting on the floor below the lowest shelf is a large glass flask. ";
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a glass flask here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A flask";
    }

    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.DrinkVerbs, NounsForMatching.Append("liquid").Append("fluid").ToArray()))
        {
            if (string.IsNullOrEmpty(LiquidColor))
                return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

            var message = "Mmmmm....that tasted just like delicious poisonous chemicals!";
            var result = new DeathProcessor().Process(message, context);
            return Task.FromResult<InteractionResult?>(result);
        }

        if (action.Match(["empty", "dump", "pour"], NounsForMatching))
        {
            if (string.IsNullOrEmpty(LiquidColor))
                return Task.FromResult<InteractionResult?>(
                    new PositiveInteractionResult("There's nothing in the glass flask. "));

            LiquidColor = null;
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult("The glass flask is now empty. "));
        }

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}