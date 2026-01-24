using System.Text;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda.BioLab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.LabOffice;

/// <summary>
/// Timer that counts down 3 turns after fungicide is activated.
/// Protects player from mutants while active. When timer expires,
/// mutants recover and will kill the player if door remains open.
/// </summary>
public class FungicideTimer : ItemBase, ITurnBasedActor
{
    private const string MistMessage =
        "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. " +
        "Horrifying biological nightmares stagger about making choking noises. ";

    private const string MistClearsMessage =
        "\nThe mist in the Bio Lab clears. The mutants recover and rush toward the door! ";

    private const string DeathMessage =
        "Mutated monsters from the Bio Lab pour into the office. You are devoured. ";

    public override string[] NounsForMatching => [];

    [UsedImplicitly] public int TurnsRemaining { get; set; } = 4;

    [UsedImplicitly] public bool IsActive { get; set; }

    [UsedImplicitly] public bool HasEverBeenActivated { get; set; }

    public void Reset()
    {
        // Set to 3 because the timer ticks on the same turn the button is pressed.
        // This gives the player 2 actual turns of protection after the button press:
        // Turn 1: Open door (mist visible), Turn 2: Wait (mist clears), Turn 3: Death
        TurnsRemaining = 3;
        IsActive = true;
        HasEverBeenActivated = true;
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        var door = Repository.GetItem<OfficeDoor>();
        var inLabOffice = context.CurrentLocation is Location.Lawanda.LabOffice.LabOffice;

        // Fungicide has worn off - mutants attack if door is open and player is in office
        if (!IsActive)
        {
            if (door.IsOpen && inLabOffice)
            {
                var deathResult = new DeathProcessor().Process(DeathMessage, context);
                return Task.FromResult(deathResult.InteractionMessage);
            }

            return Task.FromResult(string.Empty);
        }

        TurnsRemaining--;

        var message = new StringBuilder();

        // Show mist message if door is open and player is in office
        // Skip if door was just opened this turn (OnOpening already showed it)
        if (door.IsOpen && inLabOffice && !door.JustOpenedThisTurn)
            message.Append(MistMessage);

        // Clear the flag for next turn
        door.JustOpenedThisTurn = false;

        // Timer expired - mist clears
        if (TurnsRemaining <= 0)
        {
            IsActive = false;
            if (door.IsOpen && inLabOffice)
                message.Append(MistClearsMessage);

            // If player is in Bio Lab when fungicide wears off, start the chase
            if (context.CurrentLocation is BioLabLocation bioLab && !bioLab.ChaseStarted)
            {
                bioLab.ChaseStarted = true;
                var chaseManager = Repository.GetItem<ChaseSceneManager>();
                chaseManager.StartChase(bioLab);

                if (!context.Actors.Contains(chaseManager))
                    context.RegisterActor(chaseManager);
            }
        }

        return Task.FromResult(message.ToString());
    }
}