using ChatLambda;
using Planetfall.Item.Lawanda.PlanetaryDefense;
using Planetfall.Location.Lawanda;

namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class FloydLocationBehaviors(Floyd floyd)
{
    /// <summary>
    /// The ways a player names the Repair Room's little opening. The Floyd Lambda has no room
    /// context, so "go through the little door" comes back as GoSomewhere with direction
    /// "little door", not "north" - only this layer knows the opening lies north, so only this
    /// layer can accept the phrasings for the thing the player can actually see (issue #562
    /// follow-up). "n" is accepted as defense in depth: the prompt normalizes abbreviations, but a
    /// model that doesn't (gpt-5.4 was measured emitting "n") must not silently break the puzzle.
    /// </summary>
    private static readonly HashSet<string> SmallDoorDirections = new(StringComparer.OrdinalIgnoreCase)
    {
        "north", "n", "door", "little door", "small door", "opening", "small opening", "little opening"
    };

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
            SmallDoorDirections.Contains(
                companionResponse.Metadata.Parameters?.FirstOrDefault().Value?.ToString()?.Trim() ?? ""))
            return HandleSmallDoorExploration(context);

        return null;
    }

    private string HandleFromitzBoardRetrieval(IContext context)
    {
        floyd.SkipActingThisTurn(context);

        // Only actually (re-)grant the board the first time. On repeat visits, the board may already
        // be in another room, or - worse - installed and doing puzzle duty in the FromitzAccessPanel;
        // context.ItemPlacedHere() would silently rip it out of wherever it currently is (issue #360).
        if (floyd.HasGottenTheFromitzBoard)
            return FloydConstants.AlreadyGotTheFromitzBoard;

        // The board is not in play until Floyd has been through the little door and found it: the
        // original keeps it INVISIBLE until then (comptwo.zabstr:68) and answers "What fromitz
        // board?" (compone.zil:1904). Granting it regardless let a player skip the discovery
        // sequence entirely. Checked AFTER the already-got guard, mirroring the original's order, so
        // a save that somehow holds the board without the flag still gets "already did that".
        if (!floyd.HasEverGoneThroughTheLittleDoor)
            return FloydConstants.WhatFromitzBoard;

        context.ItemPlacedHere<ShinyFromitzBoard>();
        floyd.HasGottenTheFromitzBoard = true;
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
