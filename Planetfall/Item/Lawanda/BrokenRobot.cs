using GameEngine.Location;
using Model.Location;

namespace Planetfall.Item.Lawanda;

public class BrokenRobot : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["broken robot", "robot", "damaged robot"];

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Lying face down at the bottom of the stairs is a motionless robot. It appears to be damaged beyond repair. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return NeverPickedUpDescription(currentLocation!);
    }

    // Issue #367: without this, "examine robot" fell through to the engine's generic
    // "nothing special" fallback right after Floyd's Achilles eulogy (FloydConstants.Achilles),
    // which is jarring since Floyd just named the fall and the bad foot specifically.
    string ICanBeExamined.ExaminationDescription =>
        "It's Achilles, lying face down just as Floyd said. One foot is twisted at an odd angle -- the same foot he always had trouble with. ";

    public override void OnBeingExamined(IContext context)
    {
    }

    // BrokenRobot never implemented ICanBeTakenAndDropped, so "take robot" fell through to
    // an AI-generated, non-deterministic refusal. Giving it a fixed CannotBeTakenDescription
    // routes through CannotBeTakenProcessor instead, matching the solemnity of Floyd's eulogy.
    public override string? CannotBeTakenDescription =>
        "Whatever happened to Achilles, it doesn't seem right to carry him off like scrap. You leave him as Floyd found him. ";
}
