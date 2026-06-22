using GameEngine.IntentEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class InStream : DarkLocation
{
    // The ZIL room IN-STREAM is a NONLANDBIT (water) room reached by boat, but this port already
    // models the adjacent Reservoir as a drained mud flat the player crosses on foot — no boat (see
    // Reservoir.cs / ReservoirSouth.cs). For consistency, and because the port never brings the boat
    // here (a boat-gated room would be unreachable), InStream is a plain walk-in DarkLocation. The
    // "on the gently flowing stream" text is kept as flavor. Intentional simplification (issue #210).
    public override string Name => "Stream";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            // ZIL IN-STREAM: DOWN and EAST both flow back into the Reservoir.
            { Direction.Down, new MovementParameters { Location = GetLocation<Reservoir>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Reservoir>() } },
            {
                // ZIL IN-STREAM: UP/WEST are blocked — the channel is too narrow.
                Direction.Up,
                new MovementParameters { CanGo = _ => false, CustomFailureMessage = "The channel is too narrow. " }
            },
            {
                Direction.W,
                new MovementParameters { CanGo = _ => false, CustomFailureMessage = "The channel is too narrow. " }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are on the gently flowing stream. The upstream route is too narrow to navigate, and the " +
               "downstream route is invisible due to twisting walls. There is a narrow beach to land on. ";
    }

    /// <summary>
    ///     The ZIL gives IN-STREAM a LAND exit to STREAM-VIEW. <see cref="Direction" /> has no "Land"
    ///     member and the parser maps no word to one, so handle the original LAND command (and natural
    ///     synonyms) here and route to the beach at Stream View. Issue #210, decision 2(a).
    /// </summary>
    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input,
        IContext context, IGenerationClient client)
    {
        if (string.IsNullOrEmpty(input))
            return await base.RespondToSpecificLocationInteraction(input, context, client);

        var preppedInput = input.ToLowerInvariant().Trim();

        string[] landCommands = ["land", "disembark", "shore", "beach", "ashore", "go to shore", "go ashore"];
        if (landCommands.Contains(preppedInput))
            return new PositiveInteractionResult(
                await MoveEngine.Go(context, client, new MovementParameters { Location = GetLocation<StreamView>() }));

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }

    public override void Init()
    {
    }
}
