using Game.IntentEngine;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.RiverLocation;

public class FrigidRiverThree : FrigidRiverBase
{
    protected override Dictionary<Direction, MovementParameters> Map => new();

    protected override string ContextBasedDescription =>
        "The river descends here into a valley. There is a narrow beach on the west shore below the cliffs. " +
        "In the distance a faint rumbling can be heard. ";

    public override async Task<string> Act(IContext context, IGenerationClient client)
    {
        var moveInteraction = new MovementParameters { Location = Repository.GetLocation<FrigidRiverFour>() };
        var result = await MoveEngine.Go(context, client, moveInteraction);
        return "\nThe flow of the river carries you downstream. \n\n" + result;
    }
}