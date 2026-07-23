using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Item.Kalamontee.Mech;

public class Flask : ItemBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["glass flask", "flask", "large glass flask"];

    public string? LiquidColor { get; set; }

    // Issue #469: MachineShop.FlaskUnderSpout is the room's record that the flask is genuinely under the
    // spout — the description and the dispenser buttons both trust it. "put flask under spout" sets it, but
    // nothing used to clear it, so taking the flask back left it stuck true: the room kept describing the
    // flask "under the spout" while it was in the player's hand, and a button press filled the in-hand flask.
    // Clearing it here, when the flask leaves the spout, mirrors how putting it there set it.
    public override string? OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        if (previousLocation is MachineShop machineShop)
            machineShop.FlaskUnderSpout = false;

        return base.OnBeingTaken(context, previousLocation);
    }

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