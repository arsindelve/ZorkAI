using Game.IntentEngine;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.RiverLocation;

public class FrigidRiverFour : FrigidRiverBase
{
    protected override Dictionary<Direction, MovementParameters> Map => new();

    protected override string ContextBasedDescription =>
        "The river is running faster here and the sound ahead appears to be that of rushing water. On the east " +
        "shore is a sandy beach. A small area of beach can also be seen below the cliffs on the west shore. ";

    public override string Name => "Frigid River";

    public override async Task<string> Act(IContext context, IGenerationClient client)
    {
        var moveInteraction = new MovementParameters { Location = Repository.GetLocation<FrigidRiverFive>() };
        var result = await MoveEngine.Go(context, client, moveInteraction);
        return "\nThe flow of the river carries you downstream. \n\n" + result;
    }
}