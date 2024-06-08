using Game.IntentEngine;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.RiverLocation;

public class FrigidRiverOne : FrigidRiverBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<DamBase>() } }
        };

    protected override string ContextBasedDescription =>
        "You are on the Frigid River in the vicinity of the Dam. The river flows quietly here. There is a landing on the west shore. " +
        (Boat.Items.Any() ? Environment.NewLine + Boat.ItemListDescription("magic boat") : "");

    public override async Task<string> Act(IContext context, IGenerationClient client)
    {
        var moveInteraction = new MovementParameters { Location = Repository.GetLocation<FrigidRiverTwo>() };
        var result = await MoveEngine.Go(context, client, moveInteraction);
        return "\nThe flow of the river carries you downstream. \n\n" + result;
    }
}