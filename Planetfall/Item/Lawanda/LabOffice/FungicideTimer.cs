using Model.AIGeneration;

namespace Planetfall.Item.Lawanda.LabOffice;

/// <summary>
/// Timer that counts down 50 turns after fungicide is activated.
/// Protects player from mutants while active.
/// </summary>
public class FungicideTimer : ItemBase, ITurnBasedActor
{
    public override string[] NounsForMatching => [];

    [UsedImplicitly]
    public int TurnsRemaining { get; set; } = 50;

    [UsedImplicitly]
    public bool IsActive { get; set; } = false;

    public void Reset()
    {
        TurnsRemaining = 50;
        IsActive = true;
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!IsActive)
            return Task.FromResult(string.Empty);

        TurnsRemaining--;

        if (TurnsRemaining <= 0)
        {
            IsActive = false;
            return Task.FromResult(
                "The fungicide misting system shuts off with a hiss. " +
                "The Bio Lab is no longer protected! ");
        }

        return Task.FromResult(string.Empty);
    }
}
