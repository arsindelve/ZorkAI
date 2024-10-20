using GameEngine.Item;

namespace ZorkOne.Item;

public class Canary : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase, IGivePointsWhenFirstPickedUp
{
    internal const string DestroyedMessage = "It seems to have recently had a bad experience. The mountings for " +
                                           "its jewel-like eyes are empty, and its silver beak is crumpled. " +
                                           "Through a cracked crystal window below its left wing you can see the " +
                                           "remains of intricate machinery. It is not clear what result winding " +
                                           "it would have, as the mainspring seems sprung. ";
    public bool IsDestroyed { get; set; }
    
    public override string[] NounsForMatching => ["canary", "bird", "wind-up canary"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return IsDestroyed ? "There is a broken clockwork canary here. " : "There is a golden clockwork canary here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return IsDestroyed ? "A broken clockwork canary" : "A golden clockwork canary";
    }

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 0;
    
    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 0;
}