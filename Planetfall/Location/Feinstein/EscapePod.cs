using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Item;

namespace Planetfall.Location.Feinstein;

internal class EscapePod : BaseLocation, ITurnBasedActor
{
    public byte TurnsInEscapePod { get; set; }

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.Out,
                new MovementParameters
                {
                    Location = Repository.GetLocation<DeckNine>(),
                    CanGo = _ => Repository.GetItem<BulkheadDoor>().IsOpen,
                    CustomFailureMessage = "The pod door is closed. "
                }
            },
            {
                Direction.E,
                new MovementParameters
                {
                    Location = Repository.GetLocation<DeckNine>(),
                    CanGo = _ => Repository.GetItem<BulkheadDoor>().IsOpen,
                    CustomFailureMessage = "The pod door is closed. "
                }
            }
        };

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.RegisterActor(this);
        base.OnFirstTimeEnterLocation(context);
    }

    protected override string ContextBasedDescription =>
        $"This is one of the Feinstein's primary escape pods, for use in extreme emergencies. A mass of safety " +
        $"webbing, large enough to hold several dozen people, fills half the pod. The controls are entirely automated. " +
        $"The bulkhead leading out is {(Repository.GetItem<BulkheadDoor>().IsOpen ? "open" : "closed")}.";

    public override string Name => "Escape Pod";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        TurnsInEscapePod++;

        switch (TurnsInEscapePod)
        {
            case 1:
                return Task.FromResult(
                    "The ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing. ");
            case 2:
            {
                context.RemoveActor(Repository.GetLocation<DeckNine>());
                Repository.GetItem<BulkheadDoor>().IsOpen = false;
                return Task.FromResult<string>(
                    "More powerful explosions buffet the ship. The lights flicker madly, and the escape-pod bulkhead clangs shut. ");
            }
            case 3:
                return Task.FromResult("Explosions continue to rock the ship. ");
        }

        return Task.FromResult(string.Empty);
    }

    public override void Init()
    {
        StartWithItem<BulkheadDoor>();
    }
}