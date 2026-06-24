using GameEngine;
using GameEngine.Hints;
using Model.Hints;
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

    // Known dead ends / flavor / scripted non-actions, confirmed against the source by an exhaustive
    // sweep of every location, item, and constant. Deliberately EXCLUDES the diary and the library
    // computer (those are the lore source, not dead ends) and ambiguous nouns that also name progress
    // objects ("black button" fills the flask; "window" is the bio-lock viewport) — keying those to a
    // dead-end answer would lie. Keys are lowercase substrings matched against the player's question.
    public IReadOnlyDictionary<string, string> RedHerrings { get; } = new Dictionary<string, string>
    {
        // --- sequel teases / dead-end machinery ---
        ["reactor"] =
            "The reactor elevator is a dead end. It needs an access card you never find in this game — " +
            "its buttons just say 'Nothing happens.' A wink at a sequel, nothing more.",
        ["helicopter"] =
            "The helicopter is a dead end. Its controls are covered and locked and it's rusted solid; " +
            "you never get its key (the finale hands it to you 'for the sequel').",
        ["oil can"] =
            "The oil can is a red herring — just a takeable can. There's nothing in the game to oil with it.",
        ["tin can"] =
            "The 'Spam and Egz' tin can can't be opened — there's no can opener anywhere in the game, so it's a " +
            "dead end. (A tease: it looks like food for later, but you never get to it.)",
        ["spam and egz"] =
            "The 'Spam and Egz' tin can can't be opened — there's no can opener anywhere in the game. A dead end.",
        // --- flavor objects (examine-only / jokes) ---
        ["plaque"] =
            "The plaque is just scenery — you can't take it, and it only reads out tourist-brochure text " +
            "about the valley. Pure flavor.",
        ["paddleball"] =
            "The paddleball set is a joke toy — pure flavor. It solves nothing.",
        ["tape"] =
            "The rec-area tapes are flavor — examining them just lists music and novels. Nothing to play or use.",
        ["towel"] =
            "The towel is a joke (note 'Escape Pod #42 / Don't Panic'). Reading it is all it does.",
        ["graffiti"] =
            "The graffiti is examine-only flavor — a limerick mocking Blather. No use.",
        ["brochure"] =
            "The recruitment brochure is pure joke flavor about the Patrol. It does nothing.",
        ["slime"] =
            "The slime is flavor — it oozes through your fingers (you can't take it) and scrubbing it is a " +
            "hopeless gag. Nothing to solve here.",
        ["spool"] =
            "The brown spool is labelled 'Instructions for Repairing Repair Robots,' but its contents are " +
            "never usable — flavor, not a puzzle.",
        // --- jokes that punish you ---
        ["celery"] =
            "The celery is a trap: you can't take it (the ambassador is perturbed) and eating it kills you. " +
            "It does nothing useful.",
        // --- opening-scene characters (pure flavor) ---
        ["blather"] =
            "Lt. Blather is just the petty tyrant of the opening — there's no way past him to a forbidden deck, " +
            "and his demerits don't actually matter. He exists to make your life miserable before the explosion, " +
            "nothing more.",
        ["demerit"] =
            "Don't worry about Blather's demerits — they don't actually matter to the game. Ignore them.",
        ["ambassador"] =
            "The alien ambassador is pure flavor, there to liven up the opening. You can't get anything from him — " +
            "not the translator, not the celery, not a map. Just enjoy his company.",
        ["alien"] =
            "The alien ambassador is pure flavor, there to liven up the opening. You can't get anything from him. " +
            "Just enjoy his company.",
        ["translator"] =
            "You can't get the translator from the ambassador — it isn't obtainable. It's flavor, not a puzzle.",
        // --- bathrooms ---
        ["sanfac"] =
            "The sanitary facilities are a running joke — empty, dusty fixtures with nothing to do.",
        ["bathroom"] =
            "The sanitary facilities are a running joke — empty, dusty fixtures with nothing to do.",
        // --- story beats, not puzzles ---
        ["lazarus"] =
            "Lazarus is the remains of another robot that Floyd discovers and grieves over. It's a poignant " +
            "story moment, not a puzzle — there's nothing to do with it.",
        ["breastplate"] =
            "The medical-robot breastplate is story loot from the Lazarus scene — takeable, but it isn't used " +
            "to solve anything.",
        // --- examine-only scenery ---
        ["chronometer"] =
            "The chronometer is your worn flavor watch — it just shows the time (with a 'Love, Mom and Dad' " +
            "gag). You never need to operate it.",
        ["cabinet"] =
            "The repair-room cabinets are locked scenery — there's no way in and nothing in them you need.",
        ["cubbyhole"] =
            "The cubbyholes are empty examine-only nooks. Nothing inside.",
        ["logo"] =
            "The wall logo is pure decoration. Nothing to do with it.",
        ["shelves"] =
            "The shelves are examine-only and 'pretty dusty' — nothing on them you need.",
        ["shelf"] =
            "The shelves are examine-only and 'pretty dusty' — nothing on them you need.",
        ["cell door"] =
            "The brig's cell door won't open ('No way, Jose'). The brig is a scripted holding beat, not a lock to pick.",
        ["red light"] =
            "The red light is just an indicator that the computer is malfunctioning. There's nothing to do about " +
            "the light itself.",
        // The INFIRMARY bed is a death trap (a rusty diagnostic robot straps you in and kills you) — NOT a
        // place to rest. AND-keys ("&") keep this from colliding with the dorm beds, which ARE where you
        // sleep for the survival mechanic.
        ["bed&infirmary"] = InfirmaryBedAnswer,
        ["bed&med bay"] = InfirmaryBedAnswer,
        ["bed&medbay"] = InfirmaryBedAnswer,
        ["bed&medical"] = InfirmaryBedAnswer,
        // --- unkillable creatures (NOT a flat dead end — the encounter is won by NOT fighting) ---
        ["mutant"] = MutantAnswer,
        ["monster"] = MutantAnswer,
        ["creature"] = MutantAnswer,
        // --- misconceptions about the goal ---
        ["off this planet"] = LeaveAnswer,
        ["off the planet"] = LeaveAnswer,
        ["leave the planet"] = LeaveAnswer,
        ["leave this planet"] = LeaveAnswer,
        ["escape the planet"] = LeaveAnswer,
        ["get home"] = LeaveAnswer,
        // The opening explosion is scripted and unavoidable — trying to prevent it is a dead end.
        ["save the ship"] = ExplosionAnswer,
        ["stop the explosion"] = ExplosionAnswer,
        ["prevent the explosion"] = ExplosionAnswer,
        ["explosion"] = ExplosionAnswer
    };

    private const string LeaveAnswer =
        "You don't escape this planet the way you're imagining — the helicopter and reactor are dead ends. " +
        "The way out is to finish the job: repair the systems and cure the plague, and rescue comes to you.";

    private const string ExplosionAnswer =
        "You can't save the ship — the explosion is scripted and unavoidable. Stop fighting it: your only " +
        "job in those opening moments is to reach the escape pod and get off.";

    private const string InfirmaryBedAnswer =
        "Stay out of the bed in the infirmary — a rusty diagnostic robot straps you in and kills you. It's a " +
        "death trap, not a place to rest. If you need sleep, use a dorm bunk instead.";

    private const string MutantAnswer =
        "Don't try to fight the mutations — they can't be killed, and attacking them is futile. You get past " +
        "them by outsmarting the doors (let your companion grab what's behind them) and, in the final chase, by " +
        "running for the cryo-elevator and sealing it behind you — never by fighting.";
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
