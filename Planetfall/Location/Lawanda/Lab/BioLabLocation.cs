using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

internal class BioLabLocation : LocationBase, ITurnBasedActor, IFloydDoesNotTalkHere
{
    public override string Name => "Bio Lab";

    [UsedImplicitly] public bool ChaseSceneEnabled { get; set; }

    public override void Init()
    {
        StartWithItem<RatAnt>();
        StartWithItem<Troll>();
        StartWithItem<Grue>();
        StartWithItem<Triffid>();
        StartWithItem<OfficeDoor>();
        StartWithItem<BioLockInnerDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var officeDoor = Repository.GetItem<OfficeDoor>();
        var bioLockInnerDoor = Repository.GetItem<BioLockInnerDoor>();

        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.E,
                new MovementParameters
                {
                    CanGo = _ => officeDoor.IsOpen,
                    CustomFailureMessage = "The office door is closed. ",
                    Location = GetLocation<LabOfficeLocation>()
                }
            },
            {
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ => bioLockInnerDoor.IsOpen,
                    CustomFailureMessage = "The lab door is closed. ",
                    Location = GetLocation<BioLockEast>()
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        if (context is not PlanetfallContext pfContext)
            return string.Empty;

        var lightingDesc = pfContext.LabLightsOn
            ? "bright."
            : "dim, and a faint blue glow comes from a gaping crack in the northern wall.";

        return
            $"This is a huge laboratory filled with many biological experiments. The lighting is {lightingDesc} " +
            "Some of the experiments seem to be out of control... ";
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        if (context is not PlanetfallContext pfContext)
            return base.BeforeEnterLocation(context, previousLocation);

        context.RegisterActor(this);

        // Enable chase scene
        ChaseSceneEnabled = true;

        // Register the chase scene manager
        var chaseManager = Repository.GetItem<ChaseSceneManager>();
        context.RegisterActor(chaseManager);

        // Check survival conditions
        if (pfContext.LabFlooded)
        {
            var gasMask = Repository.GetItem<GasMask>();
            var message = "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering. ";

            if (!gasMask.BeingWorn)
            {
                // Player dies!
                var deathResult = new DeathProcessor().Process(
                    message + "Unfortunately, you don't seem to be that hardy.", pfContext);
                return deathResult.InteractionMessage;
            }

            return message;
        }
        else
        {
            // Instant death without fungicide
            var deathResult = new DeathProcessor().Process(
                "The mutants attack you and rip you to shreds within seconds.", pfContext);
            return deathResult.InteractionMessage;
        }
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!ChaseSceneEnabled || context is not PlanetfallContext pfContext)
            return Task.FromResult(string.Empty);

        // This will be called by the chase scene manager
        return Task.FromResult(string.Empty);
    }
}
