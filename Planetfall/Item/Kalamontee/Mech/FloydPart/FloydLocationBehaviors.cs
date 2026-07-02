using ChatLambda;
using Planetfall.Item.Lawanda.PlanetaryDefense;
using Planetfall.Location.Lawanda;

namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class FloydLocationBehaviors(Floyd floyd)
{
    public string? HandleSpecificInteraction(CompanionResponse companionResponse, IContext context)
    {
        // For now, the only custom interaction is when Floyd is in the Repair Room
        if (context.CurrentLocation is not RepairRoom)
            return null;

        if (companionResponse.Metadata?.AssistantType == "PickUp" &&
            Repository.GetItem<ShinyFromitzBoard>().NounsForMatching.Contains(companionResponse.Metadata.Parameters
                ?.FirstOrDefault().Value?.ToString()?.ToLowerInvariant()))
            return HandleFromitzBoardRetrieval(context);

        if (companionResponse.Metadata?.AssistantType == "GoSomewhere" &&
            companionResponse.Metadata.Parameters?.FirstOrDefault().Value?.ToString() == "north")
            return HandleSmallDoorExploration(context);

        return null;
    }

    private string HandleFromitzBoardRetrieval(IContext context)
    {
        // Only actually (re-)grant the board the first time. On repeat visits, the board may already
        // be in another room, or - worse - installed and doing puzzle duty in the FromitzAccessPanel;
        // context.ItemPlacedHere() would silently rip it out of wherever it currently is (issue #360).
        if (floyd.HasGottenTheFromitzBoard)
        {
            floyd.SkipActingThisTurn(context);
            return FloydConstants.AlreadyGotTheFromitzBoard;
        }

        context.ItemPlacedHere<ShinyFromitzBoard>();

        floyd.HasGottenTheFromitzBoard = true;
        floyd.SkipActingThisTurn(context);
        return FloydConstants.GetTheFromitzBoard;
    }

    private string HandleSmallDoorExploration(IContext context)
    {
        var returnString = floyd.HasEverGoneThroughTheLittleDoor
            ? "\"Not again,\" whines Floyd. "
            : FloydConstants.GoNorth;

        floyd.HasEverGoneThroughTheLittleDoor = true;
        floyd.SkipActingThisTurn(context);
        return returnString;
    }
}
