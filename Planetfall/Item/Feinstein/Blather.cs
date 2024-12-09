namespace Planetfall.Item.Feinstein;

public class Blather : ItemBase, IAmANamedPerson
{
    public override string[] NounsForMatching => ["blather", "ensign blather"];

    public string ExaminationDescription =>
        "Ensign Blather is a tall, beefy officer with a tremendous, misshapen nose. His uniform is perfect in " +
        "every respect, and the crease in his trousers could probably slice diamonds in half. ";

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        if (!action.MatchNounTwo(NounsForMatching))
            return base.RespondToMultiNounInteraction(action, context);

        if (action.MatchNounOne(Repository.GetItem<Brush>().NounsForMatching) &&
            action.MatchVerb(["throw", "toss", "launch"]))
        {
            if (!context.HasItem<Brush>())
            {
                return base.RespondToMultiNounInteraction(action, context);
            }

            context.Drop(Repository.GetItem<Brush>());

            var result =
                "The Patrol-issue self-contained multi-purpose scrub brush bounces off Blather's bulbous nose. " +
                "He becomes livid, orders you to do five hundred push-ups, gives you ten thousand demerits, " +
                "and assigns you five years of extra galley duty. ";


            return new PositiveInteractionResult(result);
        }

        return base.RespondToMultiNounInteraction(action, context);
    }
}