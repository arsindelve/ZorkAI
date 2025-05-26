using GameEngine.Location;
using Model.Intent;
using Model.AIGeneration;
using Model.Interface;
using Model.Location;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda;

namespace Planetfall.Location.Lawanda;

internal class RepairRoom : LocationBase
{
    public override string Name => "Repair Room";

    [UsedImplicitly] public bool HasToldMeAboutAchilles { get; set; }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<SystemsCorridorWest>() },
            { Direction.Up, Go<SystemsCorridorWest>() },
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "It is a robot-sized doorway -- a bit too small for you. "
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are in a dimly lit room, filled with strange machines and wide storage cabinets, all locked. To the south, a narrow " +
            "stairway leads upward. On the north wall of the room is a very small doorway. ";
    }

    public override void Init()
    {
        StartWithItem<BrokenRobot>();
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["examine"], ["cabinets", "cabinet", "storage cabinets", "storage cabinet"]))
        {
            return new PositiveInteractionResult("The cabinets are locked. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        if (HasToldMeAboutAchilles || GetItem<Floyd>().CurrentLocation != this)
            return base.AfterEnterLocation(context, previousLocation, generationClient);

        HasToldMeAboutAchilles = true;
        
        return Task.FromResult(
            "\nFloyd points at the fallen robot. \"That's Achilles. He was in charge of repairing machinery. " +
            "He repaired Floyd once. I never liked him much; he wasn't friendly like other robots. Looks like " +
            "he fell down the stairs. He always had trouble with one of his feet working right. A " +
            "Planner-person once told me that's why they named him Achilles.\"");
    }
}

// Floyd shrugs. "If you say so." He vanishes for a few minutes, and returns holding the fromitz board. It seems to be in good
// shape. He tosses it toward you, and you just manage to catch it before it smashes.

// Floyd squeezes through the opening and is gone for quite a while. You hear thudding noises and squeals of enjoyment. After
// a while the noise stops, and Floyd emerges, looking downcast. "Floyd found a rubber ball inside. Lots of fun for a while,
// but must have been old, because it fell apart. Nothing else interesting inside. Just a shiny fromitz board."

// "Not again," whines Floyd.

