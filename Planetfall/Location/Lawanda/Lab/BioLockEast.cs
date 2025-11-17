using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

internal class BioLockEast : LocationBase, ITurnBasedActor
{
    public override string Name => "Bio Lock East";

    public BioLockStateMachineManager StateMachine { get; } = new();
    
    public override void Init()
    {
        StartWithItem<BioLockInnerDoor>();
    }

    //>look through window
    // You can see a large laboratory, dimly illuminated. A blue glow comes from a crack in the northern wall of the lab. Shadowy,
    // ominous shapes move about within the room. On the floor, just inside the door, you can see a magnetic-striped card.

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<BioLockWest>() }
        };
    }

    // Opening the door reveals a Bio-Lab full of horrible mutations. You stare at them, frozen with horror.
    // Growling with hunger and delight, the mutations march into the bio-lock and devour you.

    // Your former companion, Floyd, is lying on the ground in a pool of oil.

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "The is the second half of the sterilization chamber leading from the main lab to the Bio Lab. The door " +
            "to the east, leading to the Bio Lab, has a window. The bio lock continues to the west. ";
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        context.RegisterActor(this);
        return base.BeforeEnterLocation(context, previousLocation);
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        var floyd = Repository.GetItem<Floyd>();
        var computerRoom = Repository.GetLocation<ComputerRoom>();

        var result = StateMachine.HandleTurnAction(
            floyd.IsHereAndIsOn(context),
            computerRoom.FloydHasExpressedConcern);

        return Task.FromResult(result);
    }

    
}