using EscapeRoom.Command;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace EscapeRoom.Location;

/// <summary>
///     A deadly location - falling into the maintenance shaft is instant death.
///     There is no escape from this room. You die the moment you enter.
/// </summary>
public class MaintenanceShaft : LocationBase
{
    public override string Name => "Maintenance Shaft";

    protected override string GetContextBasedDescription(IContext context)
    {
        // This description is never actually seen - the player dies immediately on entry
        return "A deep, dark maintenance shaft. You shouldn't be here.";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // No exits - you can't leave because you're dead
        return new Dictionary<Direction, MovementParameters>();
    }

    public override void Init()
    {
        // No items here - just death
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        // Instant death - no way to survive
        var deathMessage = "You step through the doorway and immediately plummet into a bottomless maintenance shaft! " +
                          "As you fall into the darkness, you have just enough time to regret your life choices. ";

        var result = new DeathProcessor().Process(deathMessage, context);

        return Task.FromResult(result.InteractionMessage);
    }
}
