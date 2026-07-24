using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     Floyd — the multiple-purpose robot from Planetfall, waiting in bin three and desperately hoping
///     to be picked (ship.zil:279-289). He is the only correct choice: he is also the only robot who
///     will climb into the spacetruck's second seat, which the autopilot requires before it will accept
///     a course.
///     TODO (Phase 3): Floyd's full companion AI — idle chatter, wandering, conversation via
///     ICanBeTalkedTo, and the pyramid's corruption of him — is still to be ported. This is the
///     Duffy-scoped Floyd: he can be chosen, he follows, and he takes the copilot seat.
/// </summary>
public class Floyd : ShipRobot
{
    public override int BinNumber => 3;

    public override string[] NounsForMatching => ["floyd", "robot", "multiple purpose robot"];

    public override string InTheBinDescription =>
        "Bin number three holds a short robot with a hopeful expression stenciled more or less " +
        "permanently onto his face. He notices you looking and waves both arms. ";

    public override string ExaminationDescription =>
        "Floyd is a squat, multiple-purpose robot in a scuffed boron-titanium finish, with an " +
        "expression of relentless good cheer. He is, by a wide margin, the friendliest thing aboard " +
        "this ship. ";

    protected override string FollowThePlayer(IContext context)
    {
        MoveToPlayer(context);
        return "Floyd skips along behind you, humming tunelessly. ";
    }
}
