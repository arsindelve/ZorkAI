using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using ZorkOne.Location.CoalMineLocation;

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

    // The on-message is produced entirely by OnBeingTurnedOn so that a match struck in a
    // drafty room can report that it went out instantly rather than that it started to burn.
    public string NowOnText => string.Empty;

    public string NowOffText => "The match has gone out. ";

    public string AlreadyOffText => "You can't turn that off. ";

    public string AlreadyOnText => string.Empty;

    // The matchbook holds six matches (original: MATCH-COUNT runs out after six). The count
    // is incremented in OnBeingTurnedOn, so once six have been struck no more can be lit.
    public string CannotBeTurnedOnText =>
        MatchesUsed >= 6 ? "I'm afraid that you have run out of matches. " : "";

    public string OnBeingTurnedOn(IContext context)
    {
        // Striking a match always consumes one, even if a draft snuffs it out instantly.
        MatchesUsed++;

        // The lower shaft (the Drafty Room) and the Timber Room have a strong draft that
        // blows the match out the moment it is struck. (Original: I-MATCH checks HERE for
        // LOWER-SHAFT / TIMBER-ROOM in zork1/1actions.zil.)
        if (context.CurrentLocation is DraftyRoom or TimberRoom)
        {
            IsOn = false;
            context.RemoveActor(this);
            return "This room is drafty, and the match goes out instantly. ";
        }

        // A struck match burns for two turns before going out. (Original: QUEUE I-MATCH 2.)
        TurnsRemainingLit = 2;
        context.RegisterActor(this);
        return "One of the matches starts to burn. ";
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

        if (TurnsRemainingLit > 0)
        {
            TurnsRemainingLit--;
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
    
    private Task<InteractionResult?> LightMatches(IContext context)
    {
        if (IsOn)
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(AlreadyOnText));
        
        if (!string.IsNullOrEmpty(CannotBeTurnedOnText))
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(CannotBeTurnedOnText));
        
        IsOn = true;
        var message = OnBeingTurnedOn(context);

        return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(message));
    }
    
    public override Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        if (action.MatchNounOne(NounsForMatching) && 
            action.Verb.ToLowerInvariant() == "light" && 
            action.Preposition.ToLowerInvariant() == "with")
        {
            if (action.NounTwo.ToLowerInvariant() == "torch")
            {
                if (!context.HasItem<Torch>())
                {
                    return Task.FromResult<InteractionResult?>(new PositiveInteractionResult("You don't have the torch."));
                }
            }
            
            return LightMatches(context);
        }
        
        return base.RespondToMultiNounInteraction(action, context);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A matchbook" + (IsOn ? " (providing light)" : "");
    }
}
