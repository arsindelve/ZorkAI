using Planetfall.Command;

namespace Planetfall.Item.Feinstein;

public class Celery : ItemBase, ICanBeEaten
{
    public override string[] NounsForMatching => ["celery"];

    public override string? CannotBeTakenDescription =>
        "The ambassador seems perturbed by your lack of normal protocol. ";

    public string OnEating(IContext context)
    {
        return new DeathProcessor().Process(
            "Oops. Looks like Blow'k-Bibben-Gordoan metabolism is not compatible with our own. You die of all sorts of convulsions.",
            context).InteractionMessage;
    }
}