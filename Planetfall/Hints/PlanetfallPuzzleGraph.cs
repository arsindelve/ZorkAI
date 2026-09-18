using GameEngine;
using GameEngine.Hints;
using Model.Interface;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.CryoElevator;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Item.Lawanda.LabOffice;
using Planetfall.Location.Kalamontee.Admin;

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
///     read off the real game objects — inventory, door and item flags, the systems monitors — never off
///     score, which is identical for players blocked on different things.
/// </summary>
internal sealed class PlanetfallPuzzleGraph : IPuzzleGraph, IProgressMapper
{
    private static readonly HintNode[] Defs =
    {
        new("ESCAPE_POD", [], false, "Survive the explosion", "Deck Nine / Escape Pod",
            s => OffTheFeinstein(s) || IsAt(s, "Escape Pod")),
        new("LAND", ["ESCAPE_POD"], false, "Get out of the pod", "Escape Pod / Crag",
            s => OffTheFeinstein(s) && !IsAt(s, "Escape Pod")),
        new("MAGNET", ["LAND"], false, "Pick up the magnet", "Tool Room",
            s => s.IsCarrying<Magnet>()),
        new("FLOYD", ["LAND"], false, "Wake the robot", "Robot Shop",
            _ => Repository.GetItem<Floyd>().HasEverBeenOn),
        new("STEEL_KEY", ["MAGNET"], false, "The magnet and the crevice", "Admin Corridor South",
            s => s.IsCarrying<Key>()),
        new("STORAGE_WEST", ["STEEL_KEY"], false, "Open the padlocked storeroom door", "Mess Corridor",
            _ => !Repository.GetItem<Padlock>().Locked || !Repository.GetItem<Padlock>().AttachedToDoor),
        new("LADDER", ["STORAGE_WEST"], false, "Take the ladder", "Storage West",
            s => s.IsCarrying<Ladder>() || Repository.GetItem<Ladder>().IsAcrossRift),
        new("CROSS_RIFT", ["LADDER"], false, "Bridge the rift", "Admin Corridor",
            _ => Repository.GetItem<Ladder>().IsAcrossRift),
        new("UPPER_CARD", ["CROSS_RIFT"], false, "The upper-elevator access card", "Small Office",
            s => s.IsCarrying<UpperElevatorAccessCard>()),
        new("KITCHEN_CARD", ["CROSS_RIFT"], false, "The kitchen access card", "Small Office",
            s => s.IsCarrying<KitchenAccessCard>()),
        new("SHUTTLE_CARD", ["CROSS_RIFT"], false, "The shuttle access card", "Large Office",
            s => s.IsCarrying<ShuttleAccessCard>()),
        new("KITCHEN", ["KITCHEN_CARD"], false, "Get into the kitchen", "Mess Hall / Kitchen",
            _ => Repository.GetItem<KitchenDoor>().HasEverBeenOpened),
        new("LOWER_CARD", ["KITCHEN"], false, "The lower-elevator access card", "Kitchen",
            s => s.IsCarrying<LowerElevatorAccessCard>()),
        new("FLASK", ["LAND"], false, "Find the flask", "Tool Room",
            s => s.IsCarrying<Flask>()),
        new("FILL_FLASK_A", ["FLASK"], false, "Fill the flask", "Machine Shop",
            _ => Repository.GetItem<Flask>().LiquidColor is not null),
        new("OPEN_ELEVATOR", ["UPPER_CARD"], false, "Open the upper elevator", "Elevator Lobby",
            _ => Repository.GetItem<UpperElevatorLobbyDoor>().HasEverBeenOpened),
        new("TOWER_UP", ["OPEN_ELEVATOR", "FILL_FLASK_A"], false, "Reach the tower", "Upper Elevator / Tower Core",
            _ => Repository.GetItem<UpperElevatorTowerDoor>().HasEverBeenOpened),
        new("COMM_FIX", ["TOWER_UP"], true, "Repair communications (the tower fluid puzzle)", "Tower Core",
            _ => Repository.GetLocation<SystemsMonitors>().CommunicationsFixed),
        new("LOWER_ELEVATOR", ["LOWER_CARD"], false, "Take the lower elevator down", "Lower Elevator",
            _ => Repository.GetItem<LowerElevatorLobbyDoor>().HasEverBeenOpened ||
                 Repository.GetItem<LowerElevatorDoor>().HasEverBeenOpened),
        new("SHUTTLE", ["SHUTTLE_CARD", "LOWER_ELEVATOR"], false, "Ride the shuttle to Lawanda",
            "Kalamontee Platform / Alfie", s => InLawanda(s)),
        new("FROMITZ", ["SHUTTLE", "FLOYD"], false, "Get the fromitz board (Floyd fetches it)", "Repair Room",
            _ => Repository.GetItem<Floyd>().HasGottenTheFromitzBoard),
        new("DEFENSE_FIX", ["FROMITZ"], true, "Repair the planetary defense", "Planetary Defense",
            _ => Repository.GetLocation<SystemsMonitors>().PlanetaryDefenseFixed),
        new("BEDISTOR_FUSED", ["SHUTTLE"], false, "Find the fused bedistor", "Course Control", null),
        new("PLIERS", ["SHUTTLE"], false, "Get the pliers", "Tool Room",
            s => s.IsCarrying<Pliers>()),
        new("COURSE_FIX", ["BEDISTOR_FUSED", "PLIERS"], true, "Repair course control", "Course Control",
            _ => Repository.GetLocation<SystemsMonitors>().CourseControlFixed),
        new("TELEPORT_CARD", ["SHUTTLE"], false, "The teleportation access card", "Lab Storage",
            s => s.IsCarrying<TeleportationAccessCard>()),
        new("LASER", ["SHUTTLE"], false, "Arm the laser with a fresh battery", "Tool Room / Lab Storage",
            s => s.IsCarrying<Laser>() && Repository.GetItem<Laser>().HasItem<FreshBattery>()),
        new("BIOLOCK", ["SHUTTLE", "FLOYD"], false, "Floyd and the bio lab", "Bio Lock",
            _ => Repository.GetItem<Floyd>().HasDied),
        new("MINI_CARD", ["BIOLOCK"], false, "The miniaturization card", "Bio Lock East",
            s => s.IsCarrying<MiniaturizationAccessCard>()),
        new("COMPUTER_FIX", ["MINI_CARD", "LASER"], false, "Cure The Disease (destroy the microbe)",
            "Miniaturization Booth", _ => Repository.GetItem<Relay>().SpeckDestroyed),
        new("GAS_MASK", ["COMPUTER_FIX"], false, "Clear the lab office", "Lab Office",
            _ => Repository.GetItem<GasMask>().BeingWorn),
        new("MUTANT_CHASE", ["GAS_MASK"], false, "Escape the mutants to the cryo-elevator", "Cryo-Elevator",
            _ => Repository.GetItem<CryoElevatorButton>().AlreadyArrived),
        new("ENDING", ["MUTANT_CHASE"], false, "The revival", "Cryo-Anteroom", null)
    };

    private static readonly Dictionary<string, HintNode> ById = Defs.ToDictionary(n => n.Id);

    public IReadOnlyCollection<PuzzleNode> Nodes { get; } =
        Defs.Select(n => new PuzzleNode(n.Id, n.Prereqs, n.Optional, n.Title, n.Location)).ToList();

    public ProgressState Map(IContext liveState)
    {
        // 1. Verified completions.
        var verified = Defs
            .Where(n => n.Done is not null && SafeDone(n, liveState))
            .Select(n => n.Id)
            .ToHashSet();

        // 2. Back-fill: a node is done if any node that (transitively) depends on it is verified done.
        var done = new HashSet<string>(verified);
        foreach (var def in Defs)
            if (!done.Contains(def.Id) && DescendantsOf(def.Id).Any(verified.Contains))
                done.Add(def.Id);

        // 3. Status: Done | Available (prereqs all done) | Locked.
        var nodes = new Dictionary<string, NodeStatus>();
        foreach (var def in Defs)
            nodes[def.Id] = done.Contains(def.Id) ? NodeStatus.Done
                : def.Prereqs.All(done.Contains) ? NodeStatus.Available
                : NodeStatus.Locked;

        return new ProgressState(nodes, new Dictionary<string, object>());
    }

    public IReadOnlyCollection<string> OpenSet(ProgressState state)
    {
        return Defs.Where(n => state.StatusOf(n.Id) == NodeStatus.Available).Select(n => n.Id).ToList();
    }

    public IReadOnlyList<string> ActiveBlockers(ProgressState state, IContext liveState)
    {
        // Mandatory spine first — a stuck player wants the required next step, not an optional
        // side-repair — then in authored (walkthrough) order.
        return OpenSet(state)
            .OrderBy(id => ById[id].Optional ? 1 : 0)
            .ToList();
    }

    // ---- predicates ---------------------------------------------------------------------------

    private static bool SafeDone(HintNode node, IContext state)
    {
        // A predicate that throws (an object not yet created in a partial test harness, say) must read
        // as "not done", not take the whole hint system down.
        try
        {
            return node.Done!(state);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool OffTheFeinstein(IContext state)
    {
        // An unknown location reads as "nothing done yet", never as "escaped".
        var ns = state.CurrentLocation?.GetType().Namespace;
        return ns is not null && !ns.Contains(".Feinstein", StringComparison.Ordinal);
    }

    private static bool InLawanda(IContext state)
    {
        var ns = state.CurrentLocation?.GetType().Namespace ?? string.Empty;
        return ns.Contains(".Lawanda", StringComparison.Ordinal);
    }

    private static bool IsAt(IContext state, string locationName)
    {
        return string.Equals(state.CurrentLocation?.Name, locationName, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> DescendantsOf(string id)
    {
        var result = new HashSet<string>();
        var queue = new Queue<string>(Defs.Where(n => n.Prereqs.Contains(id)).Select(n => n.Id));
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!result.Add(current)) continue;
            foreach (var child in Defs.Where(n => n.Prereqs.Contains(current)).Select(n => n.Id))
                queue.Enqueue(child);
        }

        return result;
    }
}
