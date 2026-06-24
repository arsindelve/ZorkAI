using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class ReservoirSouth : DarkLocation, ITurnBasedActor
{
    public bool IsDrained { get; set; }

    public bool IsFull { get; set; }

    public bool IsDraining { get; set; }

    public bool IsFilling { get; set; }

    public int DrainingCountDown { get; set; }

    public int FillingCountDown { get; set; }

    /// <summary>
    /// Whether the reservoir is currently low enough to walk across to the opposite shore — the ZIL
    /// LOW-TIDE gate (zork1/1dungeon.zil). Owned here because this room holds the fill state; both shores
    /// read it so their "you would drown" barriers stay symmetric (issue #87).
    /// </summary>
    public bool CanCross => IsDrained || (IsFilling && !IsFull);

    public override string Name => "Reservoir South";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (IsDraining)
        {
            DrainingCountDown--;

            if (DrainingCountDown != 0)
                return Task.FromResult(string.Empty);

            IsDraining = IsFilling = IsFull = false;
            IsDrained = true;

            context.RemoveActor(this);

            if (context.CurrentLocation == this)
                return Task.FromResult(
                    "The water level is now quite low here and you could easily cross over to the other side. ");
        }

        if (IsFilling)
        {
            FillingCountDown--;

            if (FillingCountDown != 0)
                return Task.FromResult(string.Empty);

            IsFull = true;
            IsDrained = IsFilling = IsDraining = false;

            context.RemoveActor(this);

            if (context.CurrentLocation == this)
                return Task.FromResult(
                    "You notice that the water level has risen to the point that it is impossible to cross. ");
        }

        return Task.FromResult(string.Empty);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.SW, new MovementParameters { Location = GetLocation<Chasm>() } },
            { Direction.SE, new MovementParameters { Location = GetLocation<DeepCanyon>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Dam>() } },
            // ZIL RESERVOIR-SOUTH: WEST follows the stream to Stream View (unconditional). This also
            // makes good on the "path along the stream to the east or west" promised in the room's
            // own description below, which was previously a dead pointer (issue #210).
            { Direction.W, new MovementParameters { Location = GetLocation<StreamView>() } },
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => CanCross, CustomFailureMessage = "You would drown.",
                    Location = GetLocation<Reservoir>()
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        // A switch picks exactly one body, so the fill states can never double up. Order matters: during a
        // refill the reservoir stays low-tide (IsDrained) until full (issue #233), so IsFilling must be
        // checked before IsDrained. The default arm (full / indeterminate) describes deep, uncrossable
        // water, matching the blocked CanCross gate so it can never contradict it. This mirrors the
        // identical structure on ReservoirNorth (issue #87). The path exits are fixed, always appended.
        var body = this switch
        {
            { IsFilling: true } =>
                "You are in a long room, to the north of which is a wide area which was formerly a reservoir, but now is merely a stream. You notice, however, that the level of the stream is rising quickly and that before long it will be impossible to cross here.",
            { IsDraining: true } =>
                "You are in a long room. To the north is a large lake, too deep to cross. You notice, however, that the water level appears to be dropping at a rapid rate. Before long, it might be possible to cross to the other side from here.",
            { IsDrained: true } =>
                "You are in a long room, to the north of which was formerly a lake. However, with the water level lowered, there is merely a wide stream running through the center of the room.",
            _ =>
                "You are in a long room on the south shore of a large lake, far too deep and wide for crossing."
        };

        return body + " There is a path along the stream to the east or west, a steep pathway climbing " +
               "southwest along the edge of a chasm, and a path leading into a canyon to the southeast.";
    }

    public override void Init()
    {
        IsFull = true;
    }

    public void StartDraining(IContext context)
    {
        IsDrained = IsFull = IsFilling = false;
        IsDraining = true;
        DrainingCountDown = 7;
        context.RegisterActor(this);
    }

    public void StartFilling(IContext context)
    {
        // Issue #233: do NOT clear IsDrained here. LOW-TIDE (IsDrained) must persist for the whole
        // refill and only clear when the reservoir is full again — mirroring the ZIL I-RFILL daemon,
        // which is QUEUEd with a delay when the gates close and clears LOW-TIDE only when it fires
        // (at completion). Act() clears IsDrained on the turn the fill countdown reaches zero.
        IsFull = IsDraining = false;
        IsFilling = true;
        FillingCountDown = 7;
        context.RegisterActor(this);
    }
}