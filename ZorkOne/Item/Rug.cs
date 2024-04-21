using Model.AIGeneration;
using Model.Intent;

namespace ZorkOne.Item;

public class Rug : ItemBase, ICanBeExamined
{
    public bool HasBeenMovedAside { get; set; }

    public override string CannotBeTakenDescription => "The rug is extremely heavy and cannot be carried.";

    public override string[] NounsForMatching => ["rug", "carpet"];

    public string ExaminationDescription => "There's nothing special about the carpet.";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        string[] matchingVerbs = ["move", "slide"];

        if (!matchingVerbs.Contains(action.Verb.ToLowerInvariant()))
            return base.RespondToSimpleInteraction(action, context, client);

        if (HasBeenMovedAside)
            return new PositiveInteractionResult(
                "Having moved the carpet previously, you find it impossible to move it again.");

        HasBeenMovedAside = true;
        Repository.GetLocation<LivingRoom>().ItemPlacedHere(Repository.GetItem<TrapDoor>());
        return new PositiveInteractionResult(
            "With a great effort, the rug is moved to one side of the room. With the rug moved, " +
            "the dusty cover of a closed trap door appears.");
    }
}