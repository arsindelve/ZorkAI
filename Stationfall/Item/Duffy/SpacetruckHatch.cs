namespace Stationfall.Item.Duffy;

/// <summary>
///     The spacetruck's hatch. It must be shut before launch — the original vents the cabin and kills a
///     player who leaves it open (ship.zil:1164-1166) — and it cannot be opened at all in deep space
///     (ship.zil:1042-1048).
/// </summary>
public class SpacetruckHatch : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["hatch", "spacetruck hatch", "truck hatch", "door"];

    public override bool IsTransparent => true;

    public string ExaminationDescription =>
        $"A broad loading hatch in the side of the spacetruck. It is {(IsOpen ? "open" : "closed")}. ";

    public override string NowOpen(ILocation currentLocation)
    {
        return "The hatch swings open. ";
    }

    public override string NowClosed(ILocation currentLocation)
    {
        return "The hatch swings shut and seals with a pneumatic sigh. ";
    }

    /// <summary>
    ///     Once the truck is under way there is nothing outside but vacuum, so the hatch stays shut
    ///     until it docks (ship.zil:1042-1048).
    /// </summary>
    public override string? CannotBeOpenedDescription(IContext context)
    {
        return Repository.GetLocation<Spacetruck>().IsInFlight
            ? "You can't open the hatch in deep space! "
            : null;
    }

    /// <summary>
    ///     Handles open/close/examine of the hatch from a room where the item itself is out of scope
    ///     (inside the truck, and the docking bay). Matches on the verb plus any noun the hatch
    ///     advertises, so the phrasings the item claims to answer to all work. Returns null when the
    ///     input isn't about the hatch, so the caller can fall through.
    /// </summary>
    public static string? TryHandleRawCommand(string? normalizedInput, IContext context, ILocation location)
    {
        if (string.IsNullOrWhiteSpace(normalizedInput))
            return null;

        var hatch = Repository.GetItem<SpacetruckHatch>();

        string[] closeVerbs = ["close", "shut", "seal"];
        string[] openVerbs = ["open", "unseal"];
        string[] examineVerbs = ["examine", "look at", "inspect", "check", "x"];

        var noun = MatchedNoun(normalizedInput, closeVerbs.Concat(openVerbs).Concat(examineVerbs), hatch);

        if (noun is null)
            return null;

        var verb = normalizedInput[..^noun.Length].Trim();

        if (examineVerbs.Contains(verb))
            return hatch.ExaminationDescription;

        if (closeVerbs.Contains(verb))
        {
            if (!hatch.IsOpen)
                return "It is already closed. ";

            hatch.IsOpen = false;
            return hatch.NowClosed(location);
        }

        var refusal = hatch.CannotBeOpenedDescription(context);

        if (refusal is not null)
            return refusal;

        if (hatch.IsOpen)
            return "It is already open. ";

        hatch.IsOpen = true;
        return hatch.NowOpen(location);
    }

    /// <summary>
    ///     Returns the hatch noun the input ends with, when the input also starts with one of the
    ///     given verbs.
    /// </summary>
    private static string? MatchedNoun(string input, IEnumerable<string> verbs, SpacetruckHatch hatch)
    {
        var verbList = verbs.ToArray();

        return hatch.NounsForMatching
            .Where(candidate => input.EndsWith(candidate, StringComparison.InvariantCultureIgnoreCase))
            .OrderByDescending(candidate => candidate.Length)
            .FirstOrDefault(candidate =>
                verbList.Contains(input[..^candidate.Length].Trim(), StringComparer.InvariantCultureIgnoreCase));
    }

    public override void Init()
    {
    }
}
