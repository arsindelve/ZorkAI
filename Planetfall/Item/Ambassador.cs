namespace Planetfall.Item;

public class Ambassador : ItemBase, IAmANamedPerson
{
    public override string[] NounsForMatching => ["ambassador", "alien"];


    public string ExaminationDescription =>
        "The ambassador has around twenty eyes, seven of which are currently open. Half of his six legs " +
        "are retracted. Green slime oozes from multiple orifices in his scaly skin. He speaks through a " +
        "mechanical translator slung around his neck. ";
}


// The alien ambassador from the planet Blow'k-bibben-Gordo ambles toward you from down the corridor. He is munching on something resembling an enormous stalk of celery, and he leaves a trail of green slime on the deck. He stops nearby, and you wince as a pool of slime begins forming beneath him on your newly polished deck. The ambassador wheezes loudly and hands you a brochure outlining his planet's major exports.

// The ambassador asks where Admiral Smithers can be found.

// The ambassador introduces himself as Br'gun-te'elkner-ipg'nun.

// The ambassador offers you a bit of celery.

// The ambassador inquires whether you are interested in a game of Bocci.

// The ambassador grunts a polite farewell, and disappears up the gangway, leaving a trail of dripping slime.

// The ambassador remarks that all humans look alike to him.

// The ambassador recites a plea for coexistence between your races.

// The ambassador squawks frantically, evacuates a massive load of gooey slime, and rushes away.

// "The leading export of Blow'k-bibben-Gordo is the adventure game
//
//     *** PLANETFALL ***
//
//         written by S. Eric Meretzky.
//     Buy one today. Better yet, buy a thousand."
