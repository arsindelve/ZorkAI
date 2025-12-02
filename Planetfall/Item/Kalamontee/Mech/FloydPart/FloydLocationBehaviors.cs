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
            return HandleSmallDoorExploration();

        return null;
    }

    private string HandleFromitzBoardRetrieval(IContext context)
    {
        var returnString = floyd.HasGottenTheFromitzBoard
            ? "Floyd looks half-bored and half-annoyed. Floyd already did that. How about some leap-frogger?\""
            : FloydConstants.GetTheFromitzBoard;

        context.ItemPlacedHere<ShinyFromitzBoard>();

        floyd.HasGottenTheFromitzBoard = true;
        return returnString;
    }

    private string HandleSmallDoorExploration()
    {
        var returnString = floyd.HasEverGoneThroughTheLittleDoor
            ? "\"Not again,\" whines Floyd. "
            : FloydConstants.GoNorth;

        floyd.HasEverGoneThroughTheLittleDoor = true;
        return returnString;
    }
}
