using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     The forms inside the boxes in the Forms Storage Room (ship.zil BOXED-FORMS-F). You can see that
///     they are forms and you can reach the boxes, but the forms themselves stay sealed — which is the
///     joke: a room three decks tall, filled entirely with paperwork you are not authorized to touch.
/// </summary>
public class BoxedForms : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["form", "forms", "boxed forms", "sealed forms"];

    public string ExaminationDescription => "The forms are sealed inside the boxes. ";

    public override string CannotBeTakenDescription =>
        "The forms are sealed inside the boxes, and breaking a Patrol seal is an offense with its own " +
        "form. ";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchNounAndAdjective(NounsForMatching))
            return new NoNounMatchInteractionResult();

        if (action.MatchVerb(["touch", "feel", "examine", "look at", "inspect", "read"]))
            return new PositiveInteractionResult(ExaminationDescription);

        // Damaging Patrol paperwork is, of course, a violation of an Act.
        if (action.MatchVerb(["crumple", "tear", "rip", "destroy", "break", "smash", "burn", "eat"]))
            return new PositiveInteractionResult(
                "Willful destruction of Patrol forms is a violation of the Uniform Code of Paperwork, " +
                "Section Nine, and you have no wish to explain yourself at a hearing. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
