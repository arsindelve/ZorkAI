using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Location.Computer;

namespace Planetfall.Item.Computer;

/// <summary>
/// Timer that counts down 200 turns after the speck is destroyed.
/// If the player is still on the silicon strip when it expires, they are electrocuted.
/// </summary>
public class SectorActivationTimer : ItemBase, ITurnBasedActor
{
    public override string[] NounsForMatching => [];

    [UsedImplicitly]
    public int TurnsRemaining { get; set; } = 200;

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        TurnsRemaining--;

        if (TurnsRemaining <= 0)
        {
            // Timer expired - check if player is still on the silicon strip
            if (IsPlayerOnSiliconStrip(context))
            {
                context.RemoveActor(this);
                return Task.FromResult(
                    new DeathProcessor().Process(
                        "The computer comes back to life with a surge of electric current. " +
                        "Unfortunately, you are still standing on one of its circuits. ",
                        context).InteractionMessage);
            }

            // Player escaped in time
            context.RemoveActor(this);
        }

        return Task.FromResult(string.Empty);
    }

    private static bool IsPlayerOnSiliconStrip(IContext context)
    {
        return context.CurrentLocation is Station384
            or StripNearStation
            or MiddleOfStrip
            or StripNearRelay;
    }
}
