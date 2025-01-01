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

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.SW, new MovementParameters { Location = GetLocation<Chasm>() } },
            { Direction.SE, new MovementParameters { Location = GetLocation<DeepCanyon>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Dam>() } },
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => IsDrained || (IsFilling && !IsFull), CustomFailureMessage = "You would drown.",
                    Location = GetLocation<Reservoir>()
                }
            }
        };

    protected override string GetContextBasedDescription() =>
        (IsFull && !IsFilling && !IsDraining
            ? "You are in a long room on the south shore of a large lake, far too deep and wide for crossing. "
            : "") +
        (IsDrained && !IsDraining && !IsFilling
            ? "You are in a long room, to the north of which was formerly a lake. However, with the water level lowered, there is merely a wide stream running through the center of the room. "
            : "") +
        (IsDraining
            ? "You are in a long room. To the north is a large lake, too deep to cross. You notice, however, that the water level appears to be dropping at a rapid rate. Before long, it might be possible to cross to the other side from here. "
            : "") +
        (IsFilling
            ? "You are in a long room, to the north of which is a wide area which was formerly a reservoir, but now is merely a stream. You notice, however, that the level of the stream is rising quickly and that before long it will be impossible to cross here. "
            : "") +
        "There is a path along the stream to the east or west, a steep pathway climbing southwest along the edge " +
        "of a chasm, and a path leading into a canyon to the southeast.";

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
        IsFull = IsDrained = IsDraining = false;
        IsFilling = true;
        FillingCountDown = 7;
        context.RegisterActor(this);
    }
}