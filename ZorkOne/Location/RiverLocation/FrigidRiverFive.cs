using GameEngine;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;
using ZorkOne.Command;

namespace ZorkOne.Location.RiverLocation;

public class FrigidRiverFive : FrigidRiverBase
{
    protected override FrigidRiverBase CarriedToLocation => Repository.GetLocation<FrigidRiverFive>();

    protected override int TurnsUntilSweptDownstream => 3;

    public override string Name => "Frigid River";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>();
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "The sound of rushing water is nearly unbearable here. On the east shore is a large landing area. ";
    }

    public override Task<string> Act(IContext context, IGenerationClient client)
    {
        context.RemoveActor(this);

        var result = "\nUnfortunately, the magic boat doesn't provide protection from the rocks and boulders " +
                     "one meets at the bottom of waterfalls. Including this one. \n\n";

        return Task.FromResult(new DeathProcessor()
            .Process(result, context).InteractionMessage);
    }
}