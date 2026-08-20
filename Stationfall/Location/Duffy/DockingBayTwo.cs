using Model.AIGeneration;

namespace Stationfall.Location.Duffy;

/// <summary>
///     Docking Bay #2 of the space station — journey's end for the Duffy segment and the doorway to the
///     rest of the game. Bay one is occupied by the derelict alien ship, which is why the autopilot
///     defaults here (ship.zil:1197-1220).
///     TODO (Phase 4): east leads to Level Five, the station's central hub, which is not yet ported.
/// </summary>
public class DockingBayTwo : LocationBase
{
    public override string Name => "Docking Bay #2";

    public override string[] NounsForMatching => ["docking bay 2", "docking bay two", "docking bay", "bay 2"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var hatch = Repository.GetItem<SpacetruckHatch>();

        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.In,
                new MovementParameters
                {
                    GatingItem = hatch,
                    Location = Repository.GetLocation<Spacetruck>(),
                    CanGo = _ => hatch.IsOpen,
                    CustomFailureMessage = "The spacetruck's hatch is closed. "
                }
            },
            {
                Direction.E,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage =
                        "[The station beyond the docking bay is not yet implemented in this port.] "
                }
            }
        };
    }

    /// <summary>
    ///     The hatch item lives in the Cargo Bay, so it is out of scope here too; route open/close/
    ///     examine to it so a player can climb back into the docked truck.
    /// </summary>
    public override Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        var normalized = input?.ToLowerInvariant().Replace("the ", "").Trim();
        var hatchResponse = SpacetruckHatch.TryHandleRawCommand(normalized, context, this);

        if (hatchResponse is not null)
            return Task.FromResult<InteractionResult>(new PositiveInteractionResult(hatchResponse));

        return base.RespondToSpecificLocationInteraction(input, context, client);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "A cramped docking bay, its walls crowded with mooring clamps and fuel couplings. Your " +
               "spacetruck sits where the autopilot parked it. A corridor leads east into the station. " +
               "There is no one here to meet you. ";
    }

    public override void Init()
    {
    }
}
