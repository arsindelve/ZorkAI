using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda.BioLab;
using Planetfall.Item.Lawanda.LabOffice;

namespace Planetfall.Location.Lawanda.LabOffice;

internal class BioLabLocation : LocationBase
{
    public override string Name => "Bio Lab";

    [UsedImplicitly]
    public bool ChaseStarted { get; set; } 

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
            { Direction.W, Go<Lab.BioLockWest>() }
        };
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        // Check if player is wearing gas mask when fungicide is active
        var fungicideTimer = Repository.GetItem<FungicideTimer>();
        var gasMask = Repository.GetItem<GasMask>();

        if (fungicideTimer.IsActive && (!context.Items.Contains(gasMask) || !gasMask.BeingWorn))
        {
            return Task.FromResult(
                new DeathProcessor().Process(
                    "Unfortunately, you don't seem to be that hardy. ",
                    context).InteractionMessage);
        }

        // Start chase scene on first entry
        if (!ChaseStarted)
        {
            ChaseStarted = true;
            var chaseManager = Repository.GetItem<ChaseSceneManager>();
            chaseManager.StartChase(this);

            if (!context.Actors.Contains(chaseManager))
                context.RegisterActor(chaseManager);

            return Task.FromResult(
                "As you enter the Bio Lab, the mutant creatures become aware of your presence! " +
                "They begin to stir and move toward you menacingly! ");
        }

        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }
}
