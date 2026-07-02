using GameEngine.Location;
using Model.Location;
using Planetfall.Location.Lawanda;

namespace Planetfall.Item.Lawanda;

public class BrokenRobot : ItemBase, ICanBeExamined
{
    // "achilles" restores the original ZIL's SYNONYM ROBOT ACHILLES - the designed,
    // unambiguous way to refer to him once Floyd names him, since he otherwise shares
    // "robot" with Floyd's own noun list (see PR #367 review: examine/take robot can
    // disambiguate against Floyd when both are in the Repair Room).
    public override string[] NounsForMatching => ["broken robot", "robot", "damaged robot", "achilles"];

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
    //
    // The Floyd-referencing text is only valid once Floyd has actually told that story
    // (RepairRoom.HasToldMeAboutAchilles). Before that, examining/taking the robot must not
    // claim Floyd said something he never said - a player can reach the Repair Room without
    // Floyd (he wanders, can be off, or dead), so the text is gated on that flag.
    string ICanBeExamined.ExaminationDescription =>
        Repository.GetLocation<RepairRoom>().HasToldMeAboutAchilles
            ? "It's Achilles, lying face down just as Floyd said. One foot is twisted at an odd angle -- the same foot he always had trouble with. "
            : "It's a damaged robot, lying face down at the bottom of the stairs. One of its feet is twisted at an odd angle. ";

    // BrokenRobot never implemented ICanBeTakenAndDropped, so "take robot" fell through to
    // an AI-generated, non-deterministic refusal. Giving it a fixed CannotBeTakenDescription
    // routes through CannotBeTakenProcessor instead, matching the solemnity of Floyd's eulogy -
    // gated on HasToldMeAboutAchilles for the same reason as ExaminationDescription above.
    public override string? CannotBeTakenDescription =>
        Repository.GetLocation<RepairRoom>().HasToldMeAboutAchilles
            ? "Whatever happened to Achilles, it doesn't seem right to carry him off like scrap. You leave him as Floyd found him. "
            : "Whatever happened to this robot, it doesn't seem right to carry him off like scrap. You leave him where he fell. ";
}
