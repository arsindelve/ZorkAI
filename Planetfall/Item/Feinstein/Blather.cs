using Model.AIGeneration;

namespace Planetfall.Item.Feinstein;

internal class Blather : QuirkyCompanion, IAmANamedPerson, ITurnBasedActor
{
    [UsedImplicitly]
    public int TurnsOnDeckNine { get; set; }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "Ensign First Class Blather is standing before you, furiously scribbling demerits onto an oversized clipboard. ";
    }

    public override string[] NounsForMatching => ["blather", "ensign blather"];

    public string ExaminationDescription =>
        "Ensign Blather is a tall, beefy officer with a tremendous, misshapen nose. His uniform is perfect in " +
        "every respect, and the crease in his trousers could probably slice diamonds in half. ";

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        if (!action.MatchNounTwo(NounsForMatching))
            return await base.RespondToMultiNounInteraction(action, context);

        if (action.MatchNounOne(Repository.GetItem<Brush>().NounsForMatching) &&
            action.MatchVerb(["throw", "toss", "launch"]))
        {
            if (!context.HasItem<Brush>())
            {
                return await base.RespondToMultiNounInteraction(action, context);
            }

            context.Drop<Brush>();

            var result =
                "The Patrol-issue self-contained multi-purpose scrub brush bounces off Blather's bulbous nose. " +
                "He becomes livid, orders you to do five hundred push-ups, gives you ten thousand demerits, " +
                "and assigns you five years of extra galley duty. ";


            return new PositiveInteractionResult(result);
        }

        return await base.RespondToMultiNounInteraction(action, context);
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

    public override Task<string> Act(IContext context, IGenerationClient client)
    {
        TurnsOnDeckNine++;

        // This is when the ship blows up.  
        if (context.Moves == ExplosionCoordinator.TurnWhenFeinsteinBlowsUp)
        {
            return Task.FromResult(LeavesTheScene(context,
            "Blather, confused by this non-routine occurrence, orders you to continue scrubbing the floor, and then dashes off. "));
        }

        return TurnsOnDeckNine switch
        {
            1 => GenerateCompanionSpeech(context, client),
            2 => Task.FromResult(LeavesTheScene(context)),
            _ => Task.FromResult(string.Empty)
        };
    }

    protected override string SystemPrompt => """
                                              The user is playing the game Planetfall, and is an Ensign Seventh class aboard the Feinstein in the Stellar Patrol.
                                              You are Ensign First Class Blather, a minor character in the game. You are described this way:

                                              "Ensign Blather is a tall, beefy officer with a tremendous, misshapen nose. His uniform is perfect in " +
                                              every respect, and the crease in his trousers could probably slice diamonds in half.

                                              Here are examples of things you randomly say or do in the game: 
                                              
                                                - "You call this polishing, Ensign Seventh Class?" he sneers."We have a position for an Ensign Ninth Class in the toilet-scrubbing division, you know. Thirty demerits." He glares at you, his arms crossed. 
                                                - Ensign First Class Blather is standing before you, furiously scribbling demerits onto an oversized clipboard
                                                - Blather throws you to the deck and makes you do 20 push-ups.
                                                - Ensign Blather frowns as he spots a speck of dust on his pristine uniform. With an exaggerated sigh, he rummages through his impeccably organized pockets and retrieves a miniature lint roller. He meticulously rolls it over his already spotless uniform, nodding solemnly as if conducting a delicate operation. "
                                                - Ensign First Class Blather straightens his posture and narrows his eyes at you. He clears his throat loudly and launches into a monologue about the importance of proper hand gestures while giving orders aboard the ship.
                                                - Suddenly, Ensign Blather pulls out a small handkerchief with the Stellar Patrol emblem embroidered on it. With great ceremony, he unfolds the handkerchief and begins polishing his already blindingly shiny boots. "A true officer's life value," he grumbles, "is in the shine, Ensign Seventh Class. "
                                              """;

    public override void Init()
    {

    }
}