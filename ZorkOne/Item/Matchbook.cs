using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Matchbook
    : ItemBase,
        ICanBeRead,
        ICanBeExamined,
        ICanBeTakenAndDropped,
        IAmALightSourceThatTurnsOnAndOff,
        ITurnBasedActor,
        IPluralNoun
{
    public int TurnsRemainingLit { get; set; }

    public int MatchesUsed { get; set; }

    public override string[] NounsForMatching => ["matches", "matchbook", "match"];

    public bool IsOn { get; set; }

    public string NowOnText => "One of the matches starts to burn. ";

    public string NowOffText => "The match has gone out. ";

    public string AlreadyOffText => "You can't turn that off. ";

    public string AlreadyOnText => string.Empty;

    public string CannotBeTurnedOnText =>
        MatchesUsed == 5 ? "I'm afraid that you have run out of matches. " : "";

    public string OnBeingTurnedOn(IContext context)
    {
        TurnsRemainingLit = 1;
        MatchesUsed++;
        context.RegisterActor(this);
        return "One of the matches starts to burn.";
    }

    public void OnBeingTurnedOff(IContext context)
    {
        context.RemoveActor(this);
    }

    public string ExaminationDescription =>
        "The matchbook isn't very interesting, except for what's written on it. ";

    public string ReadDescription =>
        """
        (Close cover before striking)

        YOU too can make BIG MONEY in the exciting field of PAPER SHUFFLING!

        Mr. Anderson of Muddle, Mass. says: "Before I took this course I was a lowly bit twiddler. Now with what I learned at GUE Tech I feel really important and can obfuscate and confuse with the best."

        Dr. Blank had this to say: "Ten short days ago all I could look forward to was a dead-end job as a doctor. Now I have a promising future and make really big Zorkmids."

        GUE Tech can't promise these fantastic results to everyone. But when you earn your degree from GUE Tech, your future will be brighter.
        """;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return NeverPickedUpDescription(currentLocation);
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "There is a matchbook whose cover says \"Visit Beautiful FCD#3\" here. ";
    }

    public async Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!IsOn)
        {
            context.RemoveActor(this);
            return string.Empty;
        }

        if (TurnsRemainingLit == 1)
        {
            TurnsRemainingLit = 0;
            return string.Empty;
        }

        var result = await new TurnOnOrOffProcessor().Process(
            new SimpleIntent { Verb = "turn off", Noun = NounsForMatching.First() },
            context,
            this,
            client
        );

        return result?.InteractionMessage ?? string.Empty;
    }
    
    public override Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        if (action.MatchNounOne(NounsForMatching) && 
            action.Verb.ToLowerInvariant() == "light" && 
            action.Preposition.ToLowerInvariant() == "with")
        {
            if (action.ItemTwo is IAmALightSource || action.NounTwo.ToLowerInvariant() == "torch")
            {
                if (IsOn)
                    return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(AlreadyOnText));
                
                if (!string.IsNullOrEmpty(CannotBeTurnedOnText))
                    return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(CannotBeTurnedOnText));
                
                IsOn = true;
                OnBeingTurnedOn(context);
                
                return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(NowOnText));
            }
        }
        
        return base.RespondToMultiNounInteraction(action, context);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A matchbook" + (IsOn ? " (providing light)" : "");
    }
}
