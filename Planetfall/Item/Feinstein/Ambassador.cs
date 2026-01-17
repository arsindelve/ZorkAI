using ChatLambda;
using Model.AIGeneration;
using Model.Item;

namespace Planetfall.Item.Feinstein;

internal class Ambassador : QuirkyCompanion, ICanBeExamined, ICanBeTalkedTo
{
    private readonly ChatWithAmbassador _chatWithAmbassador = new(null);
    
    public override string[] NounsForMatching => ["ambassador", "alien"];

    public override string GenericDescription(ILocation? currentLocation)
    {
        return
            "A high-ranking ambassador from a newly contacted alien race is standing here on three of his " +
            "legs, and watching you with seven of his eyes. ";
    }

    public string ExaminationDescription =>
        "The ambassador has around twenty eyes, seven of which are currently open. Half of his six legs " +
        "are retracted. Green slime oozes from multiple orifices in his scaly skin. He speaks through a " +
        "mechanical translator slung around his neck. ";

    public override Task<string> Act(IContext context, IGenerationClient client)
    {
        // Ship begins to explode 
        if (context.Moves == ExplosionCoordinator.TurnWhenFeinsteinBlowsUp)
            return Task.FromResult(LeavesTheScene(context,
                "The ambassador squawks frantically, evacuates a massive load of gooey slime, and rushes away. "));

        Func<Task<string>> action = new Random().Next(1, 10) switch
        {
            // 20% chance the Ambassador does nothing at all.
            <= 2 => (Func<Task<string>>)(async () => await Task.FromResult(String.Empty)),
            // 20% chance he leaves
            <= 4 => (Func<Task<string>>)(async () => await Task.FromResult(LeavesTheScene(context))),
            // 60% chance he says or does something quirky. 
            _ => (Func<Task<string>>)(async () => await GenerateCompanionSpeech(context, client))
        };
        
        Task<string> chosenAction = action.Invoke();
        return chosenAction;
    }

    protected override string SystemPrompt => """
                                              The user is playing the game Planetfall, and is an Ensign Seventh class aboard the Feinstein in the Stellar Patrol.
                                              You are the "ambassador", a very minor character in the game. You are described this way:

                                              "The ambassador has around twenty eyes, seven of which are currently open. Half of his six legs
                                              are retracted. Green slime oozes from multiple orifices in his scaly skin. He speaks through a
                                              mechanical translator slung around his neck."

                                              IMPORTANT FORMATTING RULE: When the ambassador speaks, you MUST:
                                              1. Preface speech with an attribution like "The ambassador says", "The ambassador remarks", "The ambassador inquires", etc.
                                              2. Put quotes around the actual spoken words.

                                              Here are examples of things you randomly say or do in the game:

                                                - The ambassador asks, "Where can Admiral Smithers be found?"
                                                - The ambassador extends a slimy appendage and says, "I am Br'gun-te'elkner-ipg'nun."
                                                - The ambassador inquires, "Would you be interested in a game of Bocci?"
                                                - The ambassador remarks, "All humans look alike to me."
                                                - The ambassador recites solemnly, "May our races coexist in eternal harmony."
                                                - The ambassador tilts several eyes toward you and asks, "Are you performing some sort of religious ceremony?"
                                                - The ambassador oozes thoughtfully and notes, "I often confuse humans with elaborate sandwiches, especially when hungry."

                                              """;

    internal string JoinsTheScene(IContext context, ICanContainItems location)
    {
        location.ItemPlacedHere(this);
        location.ItemPlacedHere<Slime>();
        context.ItemPlacedHere<Brochure>();

        context.RegisterActor(this);

        return
            "\n\nThe alien ambassador from the planet Blow'k-bibben-Gordo ambles toward you from down the corridor. " +
            "He is munching on something resembling an enormous stalk of celery, and he leaves a trail of green " +
            "slime on the deck. He stops nearby, and you wince as a pool of slime begins forming beneath him on " +
            "your newly polished deck. The ambassador wheezes loudly and hands you a brochure outlining his planet's " +
            "major exports.";
    }

    internal string LeavesTheScene(IContext context, string? partingText = null)
    {
        CurrentLocation?.RemoveItem(this);
        CurrentLocation = null;
        context.RemoveActor(this);

        return partingText ??
               "The ambassador grunts a polite farewell, and disappears up the gangway, leaving a trail of dripping slime. \n\n";
    }

    public override void Init()
    {
        StartWithItemInside<Celery>();
    }

    public async Task<string> OnBeingTalkedTo(string text, IContext context, IGenerationClient client)
    {
        try
        {
            var response = await _chatWithAmbassador.AskAmbassadorAsync(text);
            
            // Add the response to Ambassador's conversation history for continuity
            LastTurnsOutput.Push(response.Message);
            
            return response.Message;
        }
        catch (Exception)
        {
            // Fallback to a generic Ambassador response if the service fails
            return "The ambassador makes a series of unintelligible sounds through his mechanical translator, then oozes some green slime.";
        }
    }
}