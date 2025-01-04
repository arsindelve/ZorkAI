using Model.AIGeneration;

namespace Planetfall.Item.Feinstein;

internal class Ambassador : QuirkyCompanion, ICanBeExamined
{
    public override string[] NounsForMatching => ["ambassador", "alien"];

    protected override string SystemPrompt => """
                                              The user is playing the game Planetfall, and is an Ensign Seventh class aboard the Feinstein in the Stellar Patrol.
                                              You are the "ambassador" a very minor character in the game. You are described this way:

                                              "The ambassador has around twenty eyes, seven of which are currently open. Half of his six legs 
                                                      are retracted. Green slime oozes from multiple orifices in his scaly skin. He speaks through a 
                                                      mechanical translator slung around his neck."

                                              Here are examples of things you randomly say or do in the game: 
                                              
                                                          - "The ambassador asks where Admiral Smithers can be found. ",
                                                          - "The ambassador introduces himself as Br'gun-te'elkner-ipg'nun. ",
                                                          - "The ambassador inquires whether you are interested in a game of Bocci. ",
                                                          - "The ambassador remarks that all humans look alike to him. ",
                                                          - "The ambassador recites a plea for coexistence between your races. ",
                                                          - "The ambassador asks if you are performing some sort of religious ceremony. "
                                                          - "The ambassador notes that he often confuses humans with elaborate sandwiches, especially when hungry. "


                                              """;

    public string ExaminationDescription =>
        "The ambassador has around twenty eyes, seven of which are currently open. Half of his six legs " +
        "are retracted. Green slime oozes from multiple orifices in his scaly skin. He speaks through a " +
        "mechanical translator slung around his neck. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return
            "A high-ranking ambassador from a newly contacted alien race is standing here on three of his " +
            "legs, and watching you with seven of his eyes. ";
    }

    public override Task<string> Act(IContext context, IGenerationClient client)
    {
        // Ship begins to explode 
        if (context.Moves == ExplosionCoordinator.TurnWhenFeinsteinBlowsUp)
            return Task.FromResult(LeavesTheScene(context,
                "The ambassador squawks frantically, evacuates a massive load of gooey slime, and rushes away. "));

        Func<Task<string>> action = new Random().Next(1, 10) switch
        {
            // 20% chance the Ambassador does nothing at all.
            <= 2 => (Func<Task<string>>)(async () => await Task.FromResult(string.Empty)),
            // 20% chance he leaves
            <= 4 => (Func<Task<string>>)(async () => await Task.FromResult(LeavesTheScene(context))),
            // 60% chance he says or does something quirky. 
            _ => (Func<Task<string>>)(async () => await GenerateCompanionSpeech(context, client))
        };

        Task<string> chosenAction = action.Invoke();
        return chosenAction;
    }

    internal string JoinsTheScene(IContext context, ICanHoldItems location)
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
}