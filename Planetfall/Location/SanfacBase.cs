using GameEngine.Location;

namespace Planetfall.Location;

/// <summary>
/// Base class for the sanitary-facility rooms (Sanfac A–F). They share the same destination-navigation
/// synonyms (issue #268 review: de-duplicated from six byte-identical arrays) and the same fixtures
/// scenery (issue #315); each subclass supplies its own name, exits, and description. Public because
/// <c>SanfacE</c> is public and a base type must be at least as accessible as its subclasses.
///
/// Issue #500: there used to be a SECOND class also called <c>SanfacBase</c>, in the nested
/// <c>Planetfall.Location.Kalamontee.Dorm</c> namespace, holding the fixtures scenery. Because
/// <c>SanfacA</c>–<c>SanfacD</c> live in that same nested namespace, unqualified name resolution bound
/// them to the nearer type, silently splitting the six sanfacs into two disjoint hierarchies — A–D lost
/// the navigation synonyms, E and F lost the scenery, and nothing warned. Everything common to the
/// sanfacs belongs HERE. If some subset ever needs its own prose, give that base a DISTINCT name (e.g.
/// <c>DormSanfacBase</c>) so it can never shadow this one again.
/// </summary>
public abstract class SanfacBase : LocationWithNoStartingItems
{
    public override string[] NounsForMatching => ["bathroom", "restroom", "washroom", "toilet"];

    /// <summary>
    /// All six sanfacs answer identically here. In the original, each one declares the same
    /// fixtures/toilet pseudo-object bound to a single routine, so "toilet" is a synonym of "fixtures"
    /// rather than a separate object — hence one entry carrying both nouns. (Sanfac F's description
    /// disclaims <em>bathing</em> fixtures, not the toilet fixtures described here.)
    /// </summary>
    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["fixtures", "fixture", "toilet", "toilets", "toilet bowl"],
            "The fixtures are bone dry and thick with dust. ",
            "The fixtures are plumbed into the wall. ")
    ];
}
