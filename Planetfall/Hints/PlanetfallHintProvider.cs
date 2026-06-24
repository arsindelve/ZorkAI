using GameEngine;
using GameEngine.Hints;
using Model.Interface;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Hints;

/// <summary>
///     Planetfall's implementation of the shared hint plug point (Docs/hints/07). Supplies the puzzle
///     DAG, the live-state progress mapper, the authored rung corpus, the lore + mechanic answerers,
///     the soft-lock and survival rules, and the (shared snarky-narrator) persona.
/// </summary>
public sealed class PlanetfallHintProvider : IHintProvider
{
    private static readonly PlanetfallPuzzleGraph Graph = new();

    public IPuzzleGraph PuzzleGraph => Graph;
    public IProgressMapper ProgressMapper => Graph; // the graph also maps state (it owns the node defs)
    public IHintCorpus PuzzleCorpus { get; } = new PlanetfallCorpus();
    public ILoreSource LoreSource { get; } = new PlanetfallLoreSource();
    public IMechanicExplainer Mechanics { get; } = new PlanetfallMechanicExplainer();
    public IReadOnlyList<ISoftLockRule> SoftLockRules { get; } = PlanetfallRules.SoftLocks;
    public IReadOnlyList<IProactiveRule> ProactiveRules { get; } = PlanetfallRules.Proactive;
    public HintPersona Persona => HintPersonas.SnarkyNarrator;
}

/// <summary>
///     A single puzzle node + how to tell (from live state) whether it's done. <see cref="Done" /> is
///     null for nodes whose completion flag we haven't wired yet — those are back-filled from any
///     verified deeper node that depends on them (you can't have fixed communications without first
///     reaching the tower, etc.). This is the "verified nodes now, rest iteratively" mapper.
/// </summary>
internal sealed record HintNode(string Id, string[] Prereqs, bool Optional, string Title, string Location,
    Func<IContext, bool>? Done);

internal sealed class PlanetfallPuzzleGraph : IPuzzleGraph, IProgressMapper
{
    // Verified completion predicates (flags confirmed in the Planetfall source). Long-tail nodes have
    // null and are back-filled; fill them in puzzle-by-puzzle with tests.
    private static readonly HintNode[] Defs =
    {
        new("ESCAPE_POD", [], false, "Survive the explosion", "Escape Pod", null),
        new("LAND", ["ESCAPE_POD"], false, "Get out of the pod", "Crag", null),
        new("MAGNET", ["LAND"], false, "Pick up the magnet", "Tool Room", null),
        new("FLOYD", ["LAND"], false, "Wake the robot", "Robot Shop",
            _ => Repository.GetItem<Floyd>().HasEverBeenOn),
        new("STEEL_KEY", ["MAGNET"], false, "The magnet & the crevice", "Admin Corridor South", null),
        new("STORAGE_WEST", ["STEEL_KEY"], false, "Open the padlocked door", "Storage West", null),
        new("LADDER", ["STORAGE_WEST"], false, "Take the ladder", "Storage West", null),
        new("CROSS_RIFT", ["LADDER"], false, "Bridge the rift", "Admin", null),
        new("UPPER_CARD", ["CROSS_RIFT"], false, "Upper-elevator card", "Small Office", null),
        new("KITCHEN_CARD", ["CROSS_RIFT"], false, "Kitchen card", "Small Office", null),
        new("SHUTTLE_CARD", ["CROSS_RIFT"], false, "Shuttle card", "Large Office", null),
        new("KITCHEN", ["KITCHEN_CARD"], false, "Into the kitchen", "Kitchen", null),
        new("LOWER_CARD", ["KITCHEN"], false, "Lower-elevator card", "Kitchen", null),
        new("FLASK", ["LAND"], false, "Find the flask", "Tool Room", null),
        new("FILL_FLASK_A", ["FLASK"], false, "Fill the flask", "Machine Shop", null),
        new("OPEN_ELEVATOR", ["UPPER_CARD"], false, "Open the upper elevator", "Elevator Lobby", null),
        new("TOWER_UP", ["OPEN_ELEVATOR", "FILL_FLASK_A"], false, "Reach the tower", "Tower Core", null),
        new("COMM_FIX", ["TOWER_UP"], true, "Repair communications", "Tower Core",
            _ => Repository.GetLocation<SystemsMonitors>().CommunicationsFixed),
        new("LOWER_ELEVATOR", ["LOWER_CARD"], false, "Take the lower elevator", "Kalamontee Platform", null),
        new("SHUTTLE", ["SHUTTLE_CARD", "LOWER_ELEVATOR"], false, "Ride the shuttle to Lawanda", "Lawanda", null),
        new("FROMITZ", ["SHUTTLE", "FLOYD"], false, "Get the fromitz board", "Repair Room",
            _ => Repository.GetItem<Floyd>().HasGottenTheFromitzBoard),
        new("DEFENSE_FIX", ["FROMITZ"], true, "Repair meteor defense", "Planetary Defense",
            _ => Repository.GetLocation<SystemsMonitors>().PlanetaryDefenseFixed),
        new("BEDISTOR_FUSED", ["SHUTTLE"], false, "Find the fused bedistor", "Course Control", null),
        new("PLIERS", ["SHUTTLE"], false, "Get the pliers", "Tool Room", null),
        new("COURSE_FIX", ["BEDISTOR_FUSED", "PLIERS"], true, "Repair course control", "Course Control",
            _ => Repository.GetLocation<SystemsMonitors>().CourseControlFixed),
        new("TELEPORT_CARD", ["SHUTTLE"], false, "The teleportation card", "Lab Storage", null),
        new("LASER", ["SHUTTLE"], false, "Arm the laser", "Tool Room", null),
        new("BIOLOCK", ["SHUTTLE", "FLOYD"], false, "Floyd and the bio lab", "Bio Lock", null),
        new("MINI_CARD", ["BIOLOCK"], false, "The miniaturization card", "Bio Lock East", null),
        new("COMPUTER_FIX", ["MINI_CARD", "LASER"], false, "Cure The Disease", "Microbe strip",
            _ => Repository.GetItem<Relay>().SpeckDestroyed),
        new("GAS_MASK", ["COMPUTER_FIX"], false, "Clear the lab office", "Lab Office", null),
        new("MUTANT_CHASE", ["GAS_MASK"], false, "Escape to the cryo-elevator", "Cryo-Elevator", null),
        new("ENDING", ["MUTANT_CHASE"], false, "Revival", "Cryo-Anteroom", null)
    };

    private static readonly Dictionary<string, HintNode> ById = Defs.ToDictionary(n => n.Id);

    public IReadOnlyCollection<PuzzleNode> Nodes =>
        Defs.Select(n => new PuzzleNode(n.Id, n.Prereqs, n.Optional, n.Title, n.Location)).ToList();

    public ProgressState Map(IContext liveState)
    {
        // 1. Predicate-done set (verified flags).
        var predicateDone = Defs
            .Where(n => n.Done is not null && n.Done(liveState))
            .Select(n => n.Id)
            .ToHashSet();

        // 2. Back-fill: a node is Done if it's predicate-done, or any node that (transitively) depends
        //    on it is predicate-done. (Fixing communications implies the tower was reached, etc.)
        var done = new HashSet<string>(predicateDone);
        foreach (var def in Defs)
            if (!done.Contains(def.Id) && DescendantsOf(def.Id).Any(predicateDone.Contains))
                done.Add(def.Id);

        // 3. Status: Done | Available (prereqs all done) | Locked.
        var nodes = new Dictionary<string, NodeStatus>();
        foreach (var def in Defs)
        {
            if (done.Contains(def.Id))
                nodes[def.Id] = NodeStatus.Done;
            else if (def.Prereqs.All(done.Contains))
                nodes[def.Id] = NodeStatus.Available;
            else
                nodes[def.Id] = NodeStatus.Locked;
        }

        return new ProgressState(nodes, new Dictionary<string, object>());
    }

    public IReadOnlyCollection<string> OpenSet(ProgressState state) =>
        Defs.Where(n => state.Nodes.GetValueOrDefault(n.Id) == NodeStatus.Available).Select(n => n.Id).ToList();

    public IReadOnlyList<string> ActiveBlockers(ProgressState state, IContext liveState)
    {
        // Open, mandatory nodes first (the spine the player must clear), then optional systems —
        // a stuck player usually wants the required next step, not an optional side-repair.
        return OpenSet(state)
            .OrderBy(id => ById[id].Optional ? 1 : 0)
            .ToList();
    }

    private static IEnumerable<string> DescendantsOf(string id)
    {
        // All nodes that depend (transitively) on `id` via the prereq edges.
        var result = new HashSet<string>();
        var queue = new Queue<string>(Defs.Where(n => n.Prereqs.Contains(id)).Select(n => n.Id));
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (!result.Add(cur)) continue;
            foreach (var child in Defs.Where(n => n.Prereqs.Contains(cur)).Select(n => n.Id))
                queue.Enqueue(child);
        }

        return result;
    }
}

internal static class HintPersonas
{
    // Locked build decision §7.2: one snarky, incorporeal-narrator voice for all games, never Floyd.
    // Mirrors the existing ZorkLore assistant's tone.
    public static readonly HintPersona SnarkyNarrator = new(
        "You are the invisible, incorporeal narrator of an Infocom text adventure. Deliver the given " +
        "hint in one or two dry, lightly sarcastic sentences. Reveal only what the hint says — never " +
        "more. Stay in character; never mention that you are an AI or that hints exist.");
}
