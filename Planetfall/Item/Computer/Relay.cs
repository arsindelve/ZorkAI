using Model.AIGeneration;

namespace Planetfall.Item.Computer;

public class Relay : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["relay", "microrelay", "micro-relay"];

    /// <summary>
    /// True after the speck has been hit once by the red laser.
    /// </summary>
    [UsedImplicitly]
    public bool SpeckHit { get; set; }

    /// <summary>
    /// True after the speck has been destroyed (hit twice). Computer is now fixed.
    /// </summary>
    [UsedImplicitly]
    public bool SpeckDestroyed { get; set; }

    /// <summary>
    /// True if the relay was destroyed by a non-red laser beam. Game is unwinnable.
    /// </summary>
    [UsedImplicitly]
    public bool RelayDestroyed { get; set; }

    /// <summary>
    /// Improves aim with each miss. Increases by 12 per miss.
    /// </summary>
    [UsedImplicitly]
    public int MarksmanshipCounter { get; set; }

    public string ExaminationDescription
    {
        get
        {
            if (RelayDestroyed)
                return "The relay is now just a heap of melted plastic shards. ";

            if (SpeckDestroyed)
                return "This is a vacuum-sealed microrelay, encased in red translucent plastic. ";

            return "This is a vacuum-sealed microrelay, encased in red translucent plastic. Within, you can see that some sort of speck " +
                   "or impurity has wedged itself into the contact point of the relay, preventing it from closing. The speck, " +
                   "presumably of microscopic size, resembles a blue boulder to you in your current size. ";
        }
    }

    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["look at", "look into"], NounsForMatching))
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(ExaminationDescription));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
