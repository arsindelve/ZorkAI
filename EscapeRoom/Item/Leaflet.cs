using GameEngine.Item;

namespace EscapeRoom.Item;

public class Leaflet : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["leaflet", "paper", "instructions"];

    string ICanBeExamined.ExaminationDescription => """
                                                    WELCOME TO INTERACTIVE FICTION!

                                                    Basic commands:
                                                    - LOOK or L: See your surroundings
                                                    - EXAMINE [object]: Look closely at something
                                                    - TAKE [object]: Pick something up
                                                    - DROP [object]: Put something down
                                                    - OPEN [container]: Open a container
                                                    - UNLOCK [door] WITH [key]: Use a key
                                                    - N, S, E, W: Move in a direction
                                                    - INVENTORY or I: See what you're carrying

                                                    Good luck escaping!
                                                    """;

    string ICanBeRead.ReadDescription => ((ICanBeExamined)this).ExaminationDescription;

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "A small leaflet is on the ground. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A leaflet";
    }
}
