using Model.AIGeneration;
using Model.Location;
using Planetfall.Location.Feinstein;

namespace Planetfall.Item.Feinstein;

public class Blather : ItemBase, IAmANamedPerson, ITurnBasedActor
{
    public int TurnsOnDeckNine { get; set; }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "Ensign First Class Blather is standing before you, furiously scribbling demerits onto an oversized clipboard. ";
    }

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

            context.Drop<Brush>();

            var result =
                "The Patrol-issue self-contained multi-purpose scrub brush bounces off Blather's bulbous nose. " +
                "He becomes livid, orders you to do five hundred push-ups, gives you ten thousand demerits, " +
                "and assigns you five years of extra galley duty. ";


            return new PositiveInteractionResult(result);
        }

        return base.RespondToMultiNounInteraction(action, context);
    }

    internal string LeavesTheScene(IContext context, string? alternateText = null)
    {
        CurrentLocation?.RemoveItem(this);
        CurrentLocation = null;
        context.RemoveActor(this);
        return alternateText ?? "\n\nBlather, adding fifty more demerits for good measure, moves off in search of more young ensigns to terrorize. ";
    }

    internal string JoinsTheScene(IContext context, ICanHoldItems location)
    {
        location.ItemPlacedHere(this);
        context.RegisterActor(this);

        return
            "\n\nEnsign First Class Blather swaggers in. He studies your work with half-closed eyes. " +
            "\"You call this polishing, Ensign Seventh Class?\" he sneers. \"We have a position for an " +
            "Ensign Ninth Class in the toilet-scrubbing division, you know. Thirty demerits.\" He glares " +
            "at you, his arms crossed. ";
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        TurnsOnDeckNine++;

        // This is when the ship blows up.  
        if (context.Moves == DeckNine.TurnWhenFeinsteinBlowsUp)
        {
            return Task.FromResult(LeavesTheScene(context,
            "Blather, confused by this nonroutine occurrence, orders you to continue scrubbing the floor, and then dashes off. "));
        }

        if (TurnsOnDeckNine == 2)
            return Task.FromResult(LeavesTheScene(context));

        return Task.FromResult(string.Empty);

    }
}