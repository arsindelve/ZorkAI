using GameEngine.Location;

namespace Planetfall.Location;

/// <summary>
/// Base class for the sanitary-facility rooms (Sanfac A–F). They share the same destination-navigation
/// synonyms (issue #268 review: de-duplicated from six byte-identical arrays); each subclass supplies
/// its own name, exits, and description. Public because <c>SanfacE</c> is public and a base type must
/// be at least as accessible as its subclasses.
/// </summary>
public abstract class SanfacBase : LocationWithNoStartingItems
{
    public override string[] NounsForMatching => ["bathroom", "restroom", "washroom", "toilet"];
}
