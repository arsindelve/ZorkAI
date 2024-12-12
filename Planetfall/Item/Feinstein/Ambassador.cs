using Model.AIGeneration;
using Model.Location;
using Planetfall.Location.Feinstein;

namespace Planetfall.Item.Feinstein;

public class Ambassador : ContainerBase, ITurnBasedActor
{
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

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        // Ship begins to explode 
        if (context.Moves == ExplosionCoordinator.TurnWhenFeinsteinBlowsUp)
            return Task.FromResult(LeavesTheScene(context,
                "The ambassador squawks frantically, evacuates a massive load of gooey slime, and rushes away. "));

        Func<string>[] actions =
        [
            // Two possibilities he will say/do nothing. 
            () => "",
            () => "",
            // Two possibilities he will leave.
            () => LeavesTheScene(context),
            () => LeavesTheScene(context),
            () => "The ambassador asks where Admiral Smithers can be found. ",
            () => "The ambassador introduces himself as Br'gun-te'elkner-ipg'nun. ",
            () => "The ambassador offers you a bit of celery. ",
            () => "The ambassador inquires whether you are interested in a game of Bocci. ",
            () => "The ambassador remarks that all humans look alike to him. ",
            () => "The ambassador recites a plea for coexistence between your races. ",
            () => "The ambassador asks if you are performing some sort of religious ceremony. "
        ];

        string chosenAction = actions[Random.Shared.Next(actions.Length)].Invoke();
        return Task.FromResult("\n\n" + chosenAction);
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
               "The ambassador grunts a polite farewell, and disappears up the gangway, leaving a trail of dripping slime. ";
    }

    public override void Init()
    {
        StartWithItemInside<Celery>();
    }
}