using Model.AIGeneration;

namespace Planetfall.Location.Lawanda;

/// <summary>
/// Timer that announces cryo-chamber access 7 turns after arriving in the Auxiliary Booth.
/// </summary>
public class AuxiliaryBoothTimer : ItemBase, ITurnBasedActor
{
    public override string[] NounsForMatching => [];

    [UsedImplicitly]
    public int TurnsRemaining { get; set; } = 7;

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        TurnsRemaining--;

        if (TurnsRemaining > 0)
            return Task.FromResult(string.Empty);

        context.RemoveActor(this);
        return Task.FromResult(
            "A recorded announcement blares from the public address system. " +
            "\"Revival procedure beginning. Cryo-chamber access from Project Control Office now open.\" ");
    }
}
