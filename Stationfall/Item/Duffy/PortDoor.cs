using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     The door to port on Deck Twelve (ship.zil FAKE-DOOR-F). It never opens: the slot beside it wants
///     a validated Assignment Completion Form, and nothing aboard will validate one. The door's job is
///     to tell you that clearly enough that you stop trying and go the other way.
/// </summary>
public class PortDoor : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["door", "port door", "closed door", "sealed door"];

    public string ExaminationDescription =>
        "A heavy door leading to the rest of the Duffy. It is firmly closed, and there is a slot set " +
        "into the bulkhead beside it. ";

    public override string CannotBeTakenDescription => "The door is part of the ship. ";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchNounAndAdjective(NounsForMatching))
            return new NoNounMatchInteractionResult();

        // The refusal names the remedy, which is the point: it sends the player looking for a way to
        // validate the form rather than leaving them rattling a door with no explanation.
        if (action.MatchVerb(["open", "unlock", "unseal", "force", "pull", "push", "pry"]))
            return new PositiveInteractionResult(
                "You must insert a validated Assignment Completion Form in the slot. ");

        if (action.MatchVerb(["close", "shut"]))
            return new PositiveInteractionResult("It's already closed. ");

        if (action.MatchVerb(["knock", "knock on"]))
            return new PositiveInteractionResult(
                "You knock. Somewhere beyond the door, the business of the Patrol continues without " +
                "you. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
