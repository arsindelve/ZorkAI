using System.Text;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda.BioLab;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.LabOffice;

/// <summary>
/// State machine for fungicide protection in the Bio Lab area.
/// States: Inactive -> FullProtection -> PartialProtection -> Expired
/// Free actions (no state change): Entering BioLab, Opening lab door from BioLab
/// </summary>
public class FungicideTimer : ItemBase, ITurnBasedActor
{
    public enum FungicideState
    {
        Inactive,           // Fungicide not released
        FullProtection,     // 2 actions remaining before expiration
        PartialProtection,  // 1 action remaining before expiration
        Expired             // Fungicide worn off, mutants active
    }

    private const string MistMessage =
        "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. " +
        "Horrifying biological nightmares stagger about making choking noises. ";

    private const string MistClearsMessage =
        "\nThe mist in the Bio Lab clears. The mutants recover and rush toward the door! ";

    private const string OfficeDeathMessage =
        "Mutated monsters from the Bio Lab pour into the office. You are devoured. ";

    private const string BioLabDeathMessage =
        "The last traces of mist in the air vanish. The mutants, recovering quickly, notice you and begin salivating. " +
        "Dozens of hungry eyes fix on you as the mutations surround you and begin feasting. ";

    private const string BioLabMistMessage =
        "The air is filled with mist, which is affecting the mutants. " +
        "They appear to be stunned and confused, but are slowly recovering. ";

    public override string[] NounsForMatching => [];

    [UsedImplicitly] public FungicideState State { get; set; } = FungicideState.Inactive;

    // Track if player just entered BioLab this turn (free action)
    [UsedImplicitly] public bool JustEnteredBioLab { get; set; }

    // Track if fungicide was just activated this turn (skip first state advance)
    [UsedImplicitly] public bool JustActivatedThisTurn { get; set; }

    // Track if player tried to exit west but door was closed (free action)
    [UsedImplicitly] public bool TriedToExitWestThisTurn { get; set; }


    /// <summary>
    /// Returns true if fungicide is providing protection (not Inactive or Expired)
    /// </summary>
    public bool IsActive => State is FungicideState.FullProtection or FungicideState.PartialProtection;

    /// <summary>
    /// Activate fungicide - always resets to FullProtection state
    /// </summary>
    public void Activate()
    {
        State = FungicideState.FullProtection;
        JustActivatedThisTurn = true;
    }

    /// <summary>
    /// Mark that player just entered BioLab (grants free turn)
    /// </summary>
    public void NotifyEnteredBioLab()
    {
        JustEnteredBioLab = true;
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        var officeDoor = Repository.GetItem<OfficeDoor>();
        var labDoor = Repository.GetItem<BioLockInnerDoor>();
        var inLabOffice = context.CurrentLocation is Location.Lawanda.LabOffice.LabOffice;
        var inBioLab = context.CurrentLocation is BioLabLocation;
        var inBioLockEast = context.CurrentLocation is BioLockEast;

        switch (State)
        {
            // Already expired - check for death in office
            case FungicideState.Expired:
                switch (officeDoor.IsOpen)
                {
                    case true when inLabOffice:
                    {
                        var chaseManager = Repository.GetItem<ChaseSceneManager>();
                        // If chase is active and player just arrived (backtracking), let ChaseSceneManager handle it
                        // If player paused (stayed in same location), we handle it here
                        if (chaseManager.ChaseActive && chaseManager.LastLocation != context.CurrentLocation)
                            return Task.FromResult(string.Empty);

                        var deathResult = new DeathProcessor().Process(OfficeDeathMessage, context);
                        return Task.FromResult(deathResult.InteractionMessage);
                    }
                    default:
                        return Task.FromResult(string.Empty);
                }

            // Not active yet - nothing to do
            case FungicideState.Inactive:
                return Task.FromResult(string.Empty);
        }

        var message = new StringBuilder();

        // Check for free actions (don't advance state)
        var isFreeTurn = false;

        if (inBioLab)
        {
            // Free turn: just entered BioLab
            if (JustEnteredBioLab)
            {
                isFreeTurn = true;
                JustEnteredBioLab = false;
            }

            // Free turn: just opened lab door from inside BioLab
            if (labDoor.JustOpenedFromBioLabThisTurn)
            {
                isFreeTurn = true;
                labDoor.JustOpenedFromBioLabThisTurn = false;
                message.Append(BioLabMistMessage);
            }

            // Free turn: tried to go west but door was closed (still in BioLab)
            if (TriedToExitWestThisTurn)
            {
                isFreeTurn = true;
                TriedToExitWestThisTurn = false;
                message.Append(BioLabMistMessage);
            }
        }

        // Show mist message if in office with door open
        // Skip if door was just opened this turn (OnOpening already showed it)
        if (officeDoor.IsOpen && inLabOffice && !officeDoor.JustOpenedThisTurn)
            message.Append(MistMessage);

        // Clear the office door flag
        officeDoor.JustOpenedThisTurn = false;

        // Skip state advance on the turn the fungicide was activated
        if (JustActivatedThisTurn)
        {
            JustActivatedThisTurn = false;
            return Task.FromResult(message.ToString());
        }

        // Advance state if not a free turn
        if (!isFreeTurn)
        {
            AdvanceState();
        }

        // Handle transition to Expired
        if (State == FungicideState.Expired)
        {
            message.Append(HandleExpiration(context, officeDoor, labDoor, inLabOffice, inBioLab, inBioLockEast));
        }

        return Task.FromResult(message.ToString());
    }

    private void AdvanceState()
    {
        State = State switch
        {
            FungicideState.FullProtection => FungicideState.PartialProtection,
            FungicideState.PartialProtection => FungicideState.Expired,
            _ => State
        };
    }

    private string HandleExpiration(IContext context, OfficeDoor officeDoor, BioLockInnerDoor labDoor,
        bool inLabOffice, bool inBioLab, bool inBioLockEast)
    {
        var message = new StringBuilder();

        // In office with door open - mist clears, mutants rush to door
        if (officeDoor.IsOpen && inLabOffice)
        {
            message.Append(MistClearsMessage);
        }

        // In BioLab when expired - instant death
        if (inBioLab)
        {
            var deathResult = new DeathProcessor().Process(BioLabDeathMessage, context);
            return deathResult.InteractionMessage;
        }

        // In BioLockEast (just escaped west from BioLab) - start chase
        if (!inBioLockEast || !labDoor.IsOpen) 
            return message.ToString();
        
        var bioLab = Repository.GetLocation<BioLabLocation>();
            
        if (bioLab.ChaseStarted) 
            return message.ToString();
            
        bioLab.ChaseStarted = true;
        
        var chaseManager = Repository.GetItem<ChaseSceneManager>();
        // Pass current location (BioLockEast) as starting point, and BioLab as previous
        // (player just came from BioLab, so going back E would be backtracking)
        // ChaseSceneManager won't run this turn (just registered), so we output
        // the chase message here. On the NEXT turn, if player pauses, they die.
        chaseManager.StartChase(context.CurrentLocation, bioLab);

        context.RegisterActor(chaseManager);

        // Show both the mist vanishing AND chase message since ChaseSceneManager won't run this turn
        message.Append("The last traces of mist in the air vanish. The mutants, recovering quickly, notice you and begin salivating. ");
        message.Append(chaseManager.Chooser.Choose(ChaseSceneManager.ChaseMessages));

        return message.ToString();
    }
}
