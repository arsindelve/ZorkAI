using GameEngine;
using GameEngine.Hints;
using Model.Interface;
using Model.Location;
using Planetfall.Item.Computer;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.CryoElevator;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Item.Lawanda.LabOffice;
using Planetfall.Location.Computer;
using Planetfall.Location.Feinstein;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Tower;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Shuttle;

namespace Planetfall.Hints;

/// <summary>
///     A puzzle node plus how to tell, from live state, whether it's done. <see cref="Done" /> is null for
///     the few nodes with no clean flag of their own; those are back-filled from any verified node that
///     depends on them (you can't have fixed communications without first reaching the tower).
/// </summary>
internal sealed record HintNode(
    string Id,
    string[] Prereqs,
    bool Optional,
    string Title,
    string Location,
    Func<IContext, bool>? Done);

/// <summary>
///     The Planetfall puzzle DAG (Docs/hints/planetfall/01) and its progress mapper (02). Completion is
///     read off monotonic signals — inventory, item flags, the systems monitors, and which rooms have ever
///     been entered (<see cref="ILocation.VisitCount" />) — never off score, which is identical for players
///     blocked on different things, and never off where the player happens to be standing, which they can
///     walk away from. <c>Optional</c> marks the chains that serve only the three optional system repairs,
///     so "what do I do?" prefers the mandatory spine.
/// </summary>
internal sealed class PlanetfallPuzzleGraph : IPuzzleGraph, IProgressMapper
{
    private static readonly HintNode[] Defs =
    {
        // ---- crash & escape ----------------------------------------------------------------
        // Before the explosion nothing on Deck Nine can be done; the pod bulkhead is shut. The one
        // correct move is to wait, so it gets its own node rather than a pod hint that can't be followed.
        new("EXPLOSION", [], false, "Wait for the explosion", "Deck Nine",
            s => Repository.GetItem<BulkheadDoor>().IsOpen || Visited<EscapePod>(s)),
        new("ESCAPE_POD", ["EXPLOSION"], false, "Get into the escape pod", "Deck Nine / Escape Pod",
            s => Visited<EscapePod>(s)),
        new("POD_RIDE", ["ESCAPE_POD"], false, "Ride the pod down (strap in and wait)", "Escape Pod",
            s => Repository.GetLocation<EscapePod>().LandedSafely || Visited<Underwater>(s) || Visited<Crag>(s)),
        new("LAND", ["POD_RIDE"], false, "Get out of the landed pod", "Escape Pod / Underwater / Crag",
            s => Visited<Underwater>(s) || Visited<Crag>(s)),
        new("CLIMB", ["LAND"], false, "Climb up from the crag into the complex", "Crag / Balcony / Winding Stair",
            s => Visited<Courtyard>(s) || Visited<PlainHall>(s)),

        // ---- Kalamontee: the mandatory spine to the shuttle -------------------------------
        new("MAGNET", ["CLIMB"], false, "Pick up the magnet", "Tool Room",
            s => s.IsCarrying<Magnet>()),
        new("FLOYD_ACTIVATE", ["CLIMB"], false, "Activate Floyd, the multipurpose robot", "Robot Shop",
            _ => Repository.GetItem<Floyd>().IsOn || Repository.GetItem<Floyd>().HasEverBeenOn ||
                 Repository.GetItem<Floyd>().TurnOnCountdown < 3),
        // Activation takes a few turns to take: "I activated it and nothing happened" is its own stuck point.
        new("FLOYD", ["FLOYD_ACTIVATE"], false, "Wait for Floyd to come to life", "Robot Shop",
            _ => Repository.GetItem<Floyd>().HasEverBeenOn),
        new("STEEL_KEY", ["MAGNET"], false, "The magnet and the crevice", "Admin Corridor South",
            s => s.IsCarrying<Key>()),
        new("STORAGE_WEST", ["STEEL_KEY"], false, "Open the padlocked storeroom door", "Mess Corridor",
            _ => !Repository.GetItem<Padlock>().Locked || !Repository.GetItem<Padlock>().AttachedToDoor),
        new("LADDER", ["STORAGE_WEST"], false, "Take the ladder", "Storage West",
            s => s.IsCarrying<Ladder>() || Repository.GetItem<Ladder>().IsAcrossRift),
        new("CROSS_RIFT", ["LADDER"], false, "Bridge the rift", "Admin Corridor",
            s => Repository.GetItem<Ladder>().IsAcrossRift || Visited<AdminCorridorNorth>(s)),
        new("KITCHEN_CARD", ["CROSS_RIFT"], false, "The kitchen access card", "Small Office",
            s => s.IsCarrying<KitchenAccessCard>()),
        new("SHUTTLE_CARD", ["CROSS_RIFT"], false, "The shuttle access card", "Large Office",
            s => s.IsCarrying<ShuttleAccessCard>()),
        new("KITCHEN", ["KITCHEN_CARD"], false, "Get into the kitchen", "Mess Hall / Kitchen",
            s => Visited<Kitchen>(s)),
        new("LOWER_CARD", ["KITCHEN"], false, "The lower-elevator access card", "Kitchen",
            s => s.IsCarrying<LowerElevatorAccessCard>()),
        new("LOWER_ELEVATOR", ["LOWER_CARD"], false, "Take the lower elevator down", "Lower Elevator",
            s => Visited<KalamonteePlatform>(s)),
        new("SHUTTLE", ["SHUTTLE_CARD", "LOWER_ELEVATOR"], false, "Ride the shuttle to Lawanda",
            "Kalamontee Platform / Alfie", s => Visited<LawandaPlatform>(s) || InLawanda(s) || DockedAtLawanda(s)),

        // ---- Kalamontee: the tower chain (serves only the optional communications repair) ---
        new("UPPER_CARD", ["CROSS_RIFT"], true, "The upper-elevator access card", "Small Office",
            s => s.IsCarrying<UpperElevatorAccessCard>()),
        new("FLASK", ["CLIMB"], true, "Find the flask", "Tool Room",
            s => s.IsCarrying<Flask>()),
        new("FILL_FLASK_A", ["FLASK"], true, "Fill the flask", "Machine Shop",
            _ => Repository.GetItem<Flask>().LiquidColor is not null),
        new("OPEN_ELEVATOR", ["UPPER_CARD"], true, "Open the upper elevator", "Elevator Lobby",
            s => Visited<UpperElevator>(s)),
        new("TOWER_UP", ["OPEN_ELEVATOR", "FILL_FLASK_A"], true, "Reach the tower", "Upper Elevator / Tower Core",
            s => Visited<TowerCore>(s)),
        new("COMM_POUR_1", ["TOWER_UP"], true, "The first pour in the comm room", "Comm Room",
            _ => Repository.GetLocation<CommRoom>().CurrentColor == "gray" ||
                 Repository.GetLocation<CommRoom>().IsFixed ||
                 Repository.GetLocation<SystemsMonitors>().CommunicationsFixed),
        new("COMM_FIX", ["COMM_POUR_1"], true, "Refill the flask with the other fluid (gray button) and pour again",
            "Machine Shop / Comm Room",
            _ => Repository.GetLocation<SystemsMonitors>().CommunicationsFixed ||
                 Repository.GetLocation<CommRoom>().IsFixed),

        // ---- Lawanda: the mandatory spine to the cure ---------------------------------------
        new("LASER", ["SHUTTLE"], false, "Arm the laser (its battery is dead; fit the fresh one)", "Tool Room / Lab Storage",
            s => s.IsCarrying<Laser>() && Repository.GetItem<Laser>().HasItem<FreshBattery>()),
        new("BIOLOCK", ["SHUTTLE", "FLOYD"], false, "Floyd and the bio lab", "Bio Lock",
            _ => Repository.GetItem<Floyd>().HasDied),
        new("MINI_CARD", ["BIOLOCK"], false, "The miniaturization card", "Bio Lock East",
            s => s.IsCarrying<MiniaturizationAccessCard>()),
        // The cure is three puzzles once you're inside the computer, and a player can be stuck at any of them.
        new("MINIATURIZE", ["MINI_CARD", "LASER"], false, "Miniaturize into the computer",
            "Miniaturization Booth", s => Visited<Station384>(s) || Visited<StripNearStation>(s)),
        new("SPECK", ["MINIATURIZE"], false, "Destroy the speck on the relay (the cure)", "Strip Near Relay",
            _ => Repository.GetItem<Relay>().SpeckDestroyed),
        new("MICROBE", ["SPECK"], false, "Get past the microbe and out of the computer", "Middle of Strip",
            s => Repository.GetItem<Microbe>().Dispatched || Visited<AuxiliaryBooth>(s)),
        new("GAS_MASK", ["MICROBE"], false, "The memo in the Lab Office and the gas mask", "Lab Office",
            _ => Repository.GetItem<GasMask>().BeingWorn),
        // Sealing the elevator door starts the descent; the chase is over from that moment.
        new("MUTANT_CHASE", ["GAS_MASK"], false, "Escape the mutants to the cryo-elevator", "Cryo-Elevator",
            _ => Repository.GetItem<CryoElevatorButton>().CountdownActive ||
                 Repository.GetItem<CryoElevatorButton>().AlreadyArrived),
        new("ENDING", ["MUTANT_CHASE"], false, "The revival", "Cryo-Anteroom", null),

        // ---- the optional system repairs and their parts -------------------------------------
        new("FROMITZ", ["SHUTTLE", "FLOYD"], true, "Get the fromitz board (Floyd fetches it)", "Repair Room",
            _ => Repository.GetItem<Floyd>().HasGottenTheFromitzBoard),
        new("DEFENSE_FIX", ["FROMITZ"], true, "Repair the planetary defense", "Planetary Defense",
            _ => Repository.GetLocation<SystemsMonitors>().PlanetaryDefenseFixed),
        new("BEDISTOR_FUSED", ["SHUTTLE"], true, "Look inside the course-control cube (what's wrong with it)",
            "Course Control", null),
        // The pliers and the good bedistor are in the Kalamontee half: reachable — and sometimes taken —
        // long before the shuttle. Their prerequisite is reaching the complex, not Lawanda.
        new("PLIERS", ["CLIMB"], true, "Get the pliers", "Tool Room",
            s => s.IsCarrying<Pliers>()),
        new("GOOD_BEDISTOR", ["CLIMB"], true, "Find a good bedistor", "Storage East (off Mech Corridor North)",
            s => s.IsCarrying<GoodBedistor>()),
        new("COURSE_FIX", ["BEDISTOR_FUSED", "PLIERS", "GOOD_BEDISTOR"], true, "Repair course control",
            "Course Control", _ => Repository.GetLocation<SystemsMonitors>().CourseControlFixed),
        new("TELEPORT_CARD", ["SHUTTLE"], true, "The teleportation access card", "Lab Storage",
            s => s.IsCarrying<TeleportationAccessCard>()),
        new("TELEPORT", ["TELEPORT_CARD"], true, "Use the teleportation booths", "Booth 1 / Booth 2 / Booth 3",
            s => Visited<BoothOne>(s) || Visited<BoothTwo>(s))
    };

    private static readonly Dictionary<string, HintNode> ById = Defs.ToDictionary(n => n.Id);

    /// <summary>Every node a given node transitively depends on. The graph is static, so computed once.</summary>
    private static readonly Dictionary<string, HashSet<string>> Ancestors = Defs.ToDictionary(n => n.Id, AncestorsOf);

    public IReadOnlyCollection<PuzzleNode> Nodes { get; } =
        Defs.Select(n => new PuzzleNode(n.Id, n.Prereqs, n.Optional, n.Title, n.Location)).ToList();

    public ProgressState Map(IContext liveState)
    {
        // Verified completions, plus everything they depend on: you can't have fixed communications
        // without first reaching the tower, so a verified node vouches for all its ancestors.
        var done = new HashSet<string>();
        foreach (var node in Defs.Where(n => n.Done is not null && n.Done(liveState)))
        {
            done.Add(node.Id);
            done.UnionWith(Ancestors[node.Id]);
        }

        var nodes = new Dictionary<string, NodeStatus>();
        foreach (var def in Defs)
            nodes[def.Id] = done.Contains(def.Id) ? NodeStatus.Done
                : def.Prereqs.All(done.Contains) ? NodeStatus.Available
                : NodeStatus.Locked;

        return new ProgressState(nodes);
    }

    public IReadOnlyList<string> ActiveBlockers(ProgressState state, IContext liveState)
    {
        // The puzzle in the room the player is standing in first (they went there for a reason), then the
        // mandatory spine — a stuck player wants the required next step, not an optional side-repair —
        // then authored (walkthrough) order.
        var here = liveState.CurrentLocation?.Name ?? string.Empty;
        return Defs
            .Where(n => state.StatusOf(n.Id) == NodeStatus.Available)
            .OrderBy(n => here.Length > 0 && n.Location.Contains(here, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(n => n.Optional ? 1 : 0)
            .Select(n => n.Id)
            .ToList();
    }

    // ---- predicates ---------------------------------------------------------------------------

    /// <summary>Has the player ever entered this room? (Counting the room they are in right now.)</summary>
    private static bool Visited<TLocation>(IContext state) where TLocation : class, ILocation, new()
    {
        return state.CurrentLocation is TLocation || Repository.GetLocation<TLocation>().VisitCount > 0;
    }

    private static bool InLawanda(IContext state)
    {
        var ns = state.CurrentLocation?.GetType().Namespace;
        return ns is not null && ns.Contains(".Lawanda", StringComparison.Ordinal);
    }

    /// <summary>
    ///     Still aboard Alfie, but the ride is over: the car sits at the far (Lawanda) end of its tunnel with
    ///     the door open. Position 0 is the Kalamontee end (see ShuttleCarAlfie's exit).
    /// </summary>
    private static bool DockedAtLawanda(IContext state)
    {
        var ns = state.CurrentLocation?.GetType().Namespace;
        if (ns is null || !ns.EndsWith(".Shuttle", StringComparison.Ordinal)) return false;

        var alfie = Repository.GetLocation<AlfieControlEast>();
        return alfie.TunnelPosition > 0 && !alfie.DoorIsClosed;
    }

    private static HashSet<string> AncestorsOf(HintNode node)
    {
        var result = new HashSet<string>();
        var queue = new Queue<string>(node.Prereqs);
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            if (!result.Add(id)) continue;
            foreach (var prerequisite in ById[id].Prereqs)
                queue.Enqueue(prerequisite);
        }

        return result;
    }
}
