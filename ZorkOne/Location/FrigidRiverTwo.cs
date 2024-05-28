using Game.IntentEngine;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class FrigidRiverTwo : FrigidRiverBase
{
    public int TurnsInThisLocation { get; set; }
    
    protected override Dictionary<Direction, MovementParameters> Map => new();

    protected override string ContextBasedDescription =>
        "The river turns a corner here making it impossible to see the Dam. The White Cliffs loom on the east bank " +
        "and large rocks prevent landing on the west. ";
    
    public override async Task<string> Act(IContext context, IGenerationClient client)
    {
        TurnsInThisLocation++;
        
        if (TurnsInThisLocation == 2)
        {
            var moveInteraction = new MovementParameters { Location = Repository.GetLocation<FrigidRiverThree>() };
            var result = await MoveEngine.Go(context, client, moveInteraction);
            return "\nThe flow of the river carries you downstream. \n\n" + result;
        }

        return string.Empty;
    }
}