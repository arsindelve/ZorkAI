using Planetfall.Command;

namespace Planetfall.Item.Feinstein;

// IDoNotAppearInItemLists is the original's NDESCBIT: the celery sits in the room with the
// ambassador so it is in scope, but it is never listed in the room description.
public class Celery : ItemBase, ICanBeEaten, IDoNotAppearInItemLists
{
    public override string[] NounsForMatching => ["celery", "stalk", "stalk of celery"];

    public override string? CannotBeTakenDescription =>
        "The ambassador seems perturbed by your lack of normal protocol. ";

    public (string Message, bool WasConsumed) OnEating(IContext context)
    {
        return (new DeathProcessor().Process(
            "Oops. Looks like Blow'k-Bibben-Gordoan metabolism is not compatible with our own. You die of all sorts of convulsions.",
            context).InteractionMessage, true);
    }
}