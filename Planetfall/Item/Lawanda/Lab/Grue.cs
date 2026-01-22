using Model.AIGeneration;

namespace Planetfall.Item.Lawanda.Lab;

public class Grue : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["grue", "creature", "mutant", "monster"];

    public string ExaminationDescription =>
        "Lurking nearby is a vicious-looking creature with slavering fangs. Squinting in the light, it eyes you hungrily. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A lurking fanged creature";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return ExaminationDescription;
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // Special examine behavior when not in same room
        if (action.Match(["examine", "look at"], NounsForMatching) && CurrentLocation != context.CurrentLocation)
        {
            return new PositiveInteractionResult(
                "Grues are vicious, carnivorous beasts first introduced to Earth by a visiting alien spaceship during the late 22nd century. Grues spread throughout the galaxy alongside man. Although now extinct on all civilized planets, they still exist in some backwater corners of the galaxy. Their favorite diet is Ensigns Seventh Class, but their insatiable appetite is tempered by their fear of light. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
