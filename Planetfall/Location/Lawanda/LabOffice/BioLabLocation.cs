using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda.BioLab;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Item.Lawanda.LabOffice;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.LabOffice;

internal class BioLabLocation : LocationBase
{
    public override string Name => "Bio Lab";

    [UsedImplicitly] public bool ChaseStarted { get; set; }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a huge laboratory filled with many biological experiments. The lighting is dim, and a faint " +
               "blue glow comes from a gaping crack in the northern wall. Some of the experiments seem to be out of " +
               "control...A giant plant, teeming with poisonous tentacles, is shuffling toward you on three leg-like " +
               "stalks. Lurking nearby is a vicious-looking creature with slavering fangs. Squinting in the light, " +
               "it eyes you hungrily. Rushing toward you is an ugly, deformed humanoid, bellowing in a guttural " +
               "tongue. It brandishes a piece of lab equipment shaped somewhat like a battle axe. A ferocious " +
               "feral creature, with a hairy shelled body and a whip-like tail snaps its enormous mandibles at you. " +
               "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, " +
               "but are slowly recovering. ";
    }

    public override void Init()
    {
        StartWithItem<RatAnt>();
        StartWithItem<MutantTroll>();
        StartWithItem<MutantGrue>();
        StartWithItem<Triffid>();

        StartWithItem<OfficeDoor>();
        StartWithItem<BioLockInnerDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.E,
                new MovementParameters
                {
                    CanGo = _ => Repository.GetItem<OfficeDoor>().IsOpen,
                    CustomFailureMessage = "The office door is closed. ",
                    Location = GetLocation<LabOffice>()
                }
            },
            {
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ =>
                    {
                        var door = Repository.GetItem<BioLockInnerDoor>();
                        if (!door.IsOpen)
                        {
                            // Track that player tried to go west but couldn't (free turn for fungicide)
                            Repository.GetItem<FungicideTimer>().TriedToExitWestThisTurn = true;
                        }
                        return door.IsOpen;
                    },
                    CustomFailureMessage = "The lab door is closed. ",
                    Location = GetLocation<BioLockEast>()
                }
            }
        };
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation currentLocation)
    {
        var fungicideTimer = Repository.GetItem<FungicideTimer>();

        // If leaving to LabOffice while fungicide is active, start chase when it expires
        // (The office door must be open for player to go that way)
        if (newLocation is LabOffice && fungicideTimer.IsActive)
        {
            var bioLab = Repository.GetLocation<BioLabLocation>();
            if (!bioLab.ChaseStarted)
            {
                bioLab.ChaseStarted = true;
                var chaseManager = Repository.GetItem<ChaseSceneManager>();
                // Start chase with BioLab as LastLocation (where player currently is)
                // When they arrive at LabOffice, Act() will update locations and show chase message
                chaseManager.StartChase(bioLab);

                if (!context.Actors.Contains(chaseManager))
                    context.RegisterActor(chaseManager);
            }
        }

        base.OnLeaveLocation(context, newLocation, currentLocation);
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var fungicideTimer = Repository.GetItem<FungicideTimer>();

        // When fungicide is active, the mutants are stunned - but mist is deadly without mask
        if (fungicideTimer.IsActive)
        {
            var gasMask = Repository.GetItem<GasMask>();
            if (!context.Items.Contains(gasMask) || !gasMask.BeingWorn)
            {
                return Task.FromResult(
                    new DeathProcessor().Process(
                        "Unfortunately, you don't seem to be that hardy. ",
                        context).InteractionMessage);
            }

            // Mark this as a free turn for the fungicide state machine
            fungicideTimer.NotifyEnteredBioLab();
            return base.AfterEnterLocation(context, previousLocation, generationClient);
        }

        // Fungicide not active - check if chase is already in progress
        if (ChaseStarted)
        {
            // Re-entering during chase - let ChaseSceneManager handle it
            return base.AfterEnterLocation(context, previousLocation, generationClient);
        }

        // No fungicide and no chase - mutants attack immediately
        return Task.FromResult(
            new DeathProcessor().Process(
                "The mutants attack you and rip you to shreds within seconds. ",
                context).InteractionMessage);
    }
}
