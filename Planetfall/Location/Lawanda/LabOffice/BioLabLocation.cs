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
    public bool LightsOn { get; set; } = false;

    [UsedImplicitly]
    public bool ChaseStarted { get; set; } = false;

    protected override string GetContextBasedDescription(IContext context)
    {
        if (!LightsOn)
            return "It is pitch black. You can hear disturbing sounds of movement around you. ";

        var mutantsPresent = Repository.GetItem<RatAnt>().CurrentLocation == this ||
                             Repository.GetItem<MutantTroll>().CurrentLocation == this ||
                             Repository.GetItem<MutantGrue>().CurrentLocation == this ||
                             Repository.GetItem<Triffid>().CurrentLocation == this;

        var baseDescription =
            "This is a large biological laboratory filled with strange equipment and specimen containers. ";

        if (mutantsPresent)
        {
            baseDescription +=
                "FOUR TERRIFYING MUTANT CREATURES are here: a rat-ant, a troll, a grue, and a triffid! ";
        }

        baseDescription += "The office is to the east. A passage leads west. ";

        return baseDescription;
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
                    Location = GetLocation<LabOfficeLocation>()
                }
            },
            { Direction.W, Go<Planetfall.Location.Lawanda.Lab.BioLockWest>() }
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
                    "You enter the Bio Lab but immediately start coughing. The fungicide mist fills your lungs! " +
                    "You should have worn the gas mask. ",
                    context).InteractionMessage);
        }

        // Start chase scene on first entry
        if (!ChaseStarted)
        {
            ChaseStarted = true;
            var chaseManager = Repository.GetItem<ChaseSceneManager>();
            chaseManager.StartChase();

            if (!context.Actors.Contains(chaseManager))
                context.RegisterActor(chaseManager);

            return Task.FromResult(
                "As you enter the Bio Lab, the mutant creatures become aware of your presence! " +
                "They begin to stir and move toward you menacingly! ");
        }

        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }
}
