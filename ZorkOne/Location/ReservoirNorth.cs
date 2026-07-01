using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class ReservoirNorth : DarkLocation
{
    public override string Name => "Reservoir North";

    // No "lake" alias here: it is shared with the central Reservoir and Reservoir South, and from the
    // central bed it produced a "which shore?" prompt (issue #268 review). "go to the lake" from this
    // shore still reaches the central lake bed, which keeps the alias.

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // Issue #87: both shores must gate the crossing identically. In the original game RESERVOIR-NORTH's
        // south exit is "(SOUTH TO RESERVOIR IF LOW-TIDE ELSE \"You would drown.\")" (zork1/1dungeon.zil:1796),
        // mirroring RESERVOIR-SOUTH's north exit. The fill state lives on ReservoirSouth, so read its shared
        // CanCross gate here — keeping both shores in lockstep so they can't drift apart again.
        var south = GetLocation<ReservoirSouth>();
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.S,
                new MovementParameters
                {
                    CanGo = _ => south.CanCross,
                    CustomFailureMessage = "You would drown.",
                    Location = GetLocation<Reservoir>()
                }
            },
            { Direction.N, new MovementParameters { Location = GetLocation<AtlantisRoom>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        // Issue #87 follow-up: like the south shore, this room's body must track the reservoir's fill
        // state (owned by ReservoirSouth) — a static "water level lowered / wide stream" line contradicts
        // the "You would drown." barrier on the south exit whenever the water is high or rising.
        //
        // A switch picks exactly one body, so the states can never double up. Order matters: during a
        // refill the reservoir stays low-tide (IsDrained) until full (issue #233), so IsFilling must be
        // checked before IsDrained. The default arm (full / indeterminate) describes deep, uncrossable
        // water, which matches the blocked CanCross gate and so can never contradict it. The slimy
        // stairway to the north is a fixed exit, always appended.
        var south = GetLocation<ReservoirSouth>();
        var body = south switch
        {
            { IsFilling: true } =>
                "You are in a large cavernous room, the south of which is a wide area which was formerly a reservoir, but now is merely a stream. You notice, however, that the level of the stream is rising quickly and that before long it will be impossible to cross here.",
            { IsDraining: true } =>
                "You are in a large cavernous room. To the south is a large lake, too deep to cross. You notice, however, that the water level appears to be dropping at a rapid rate. Before long, it might be possible to cross to the other side from here.",
            { IsDrained: true } =>
                "You are in a large cavernous room, the south of which was formerly a lake. However, with the water level lowered, there is merely a wide stream running through there.",
            _ =>
                "You are in a large cavernous room on the north shore of a large lake, far too deep and wide for crossing."
        };

        return body + "\nThere is a slimy stairway leaving the room to the north.";
    }

    public override void Init()
    {
        StartWithItem<AirPump>();
    }
}