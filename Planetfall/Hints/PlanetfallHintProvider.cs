using System.Text;
using GameEngine;
using GameEngine.Hints;
using Model.Hints;
using Model.Interface;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Hints;

/// <summary>
///     Planetfall's hint plug-in for the two-tier engine. Supplies the knowledge base (LLM 1 reasons
///     over it) and a precise, DAG-localized description of the player's live situation (so LLM 1 isn't
///     guessing where they are). The DAG is the deterministic localizer; the reasoning + disclosure are
///     the two LLM calls.
/// </summary>
public sealed class PlanetfallHintProvider : IHintProvider
{
    private static readonly PlanetfallLocalizer Localizer = new();

    public string Docs => PlanetfallHintDocs.Docs;

    public HintPersona Persona => new(
        "You are the invisible, incorporeal narrator of the Infocom game Planetfall — dry, lightly " +
        "sarcastic, in character. Never mention being an AI; never break the fourth wall about 'hints'.");

    public IReadOnlyList<IProactiveRule> ProactiveRules { get; } = new IProactiveRule[]
    {
        new TiredRule(), new HungerRule(), new DiseaseRule()
    };

    public string DescribePlayerContext(IContext state) => Localizer.Describe(state);

    // ---- survival proactive rules (push channel; 0 = best, higher = worse) ----------------------

    private sealed class TiredRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext s) =>
            s is PlanetfallContext c && (int)c.Tired >= 1
                ? new ProactiveNudge("sleep", "You're getting tired — find a safe place to sleep.", 3)
                : null;
    }

    private sealed class HungerRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext s) =>
            s is PlanetfallContext c && (int)c.Hunger >= 1
                ? new ProactiveNudge("hunger", "You're getting hungry and thirsty — find food and water.", 3)
                : null;
    }

    private sealed class DiseaseRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext s) =>
            s is PlanetfallContext c && c.Day >= 4 && !Repository.GetItem<Relay>().SpeckDestroyed
                ? new ProactiveNudge("disease", "You're getting sicker by the day — the cure is in the lab.", 5)
                : null;
    }
}

// =====================================================================================
// The DAG localizer. Verified completion predicates (flags confirmed in the source) drive a
// back-filled node-status map; long-tail nodes are inferred from any verified descendant ("comms
// fixed" implies the tower chain is done). It renders a compact, accurate "where is the player"
// description for LLM 1 — NOT a hard-coded hint.
// =====================================================================================

internal enum NodeStatus { Locked, Available, Done }

internal sealed record HintNode(string Id, string[] Prereqs, bool Optional, string Title, Func<IContext, bool>? Done);

internal sealed class PlanetfallLocalizer
{
    private static readonly HintNode[] Defs =
    {
        new("ESCAPE_POD", [], false, "survive the explosion", null),
        new("LAND", ["ESCAPE_POD"], false, "get out of the pod", null),
        new("MAGNET", ["LAND"], false, "get the magnet", null),
        new("FLOYD", ["LAND"], false, "wake Floyd", _ => Repository.GetItem<Floyd>().HasEverBeenOn),
        new("STEEL_KEY", ["MAGNET"], false, "get the steel key (magnet on crevice)", null),
        new("STORAGE_WEST", ["STEEL_KEY"], false, "open the padlocked storeroom", null),
        new("LADDER", ["STORAGE_WEST"], false, "take the ladder", null),
        new("CROSS_RIFT", ["LADDER"], false, "bridge the rift with the ladder", null),
        new("UPPER_CARD", ["CROSS_RIFT"], false, "get the upper-elevator card", null),
        new("KITCHEN_CARD", ["CROSS_RIFT"], false, "get the kitchen card", null),
        new("SHUTTLE_CARD", ["CROSS_RIFT"], false, "get the shuttle card", null),
        new("KITCHEN", ["KITCHEN_CARD"], false, "get into the kitchen", null),
        new("LOWER_CARD", ["KITCHEN"], false, "get the lower-elevator card", null),
        new("FLASK", ["LAND"], false, "get and fill the flask", null),
        new("OPEN_ELEVATOR", ["UPPER_CARD"], false, "open the upper elevator", null),
        new("TOWER_UP", ["OPEN_ELEVATOR", "FLASK"], false, "reach the tower", null),
        new("COMM_FIX", ["TOWER_UP"], true, "repair communications (tower fluid)",
            _ => Repository.GetLocation<SystemsMonitors>().CommunicationsFixed),
        new("LOWER_ELEVATOR", ["LOWER_CARD"], false, "take the lower elevator", null),
        new("SHUTTLE", ["SHUTTLE_CARD", "LOWER_ELEVATOR"], false, "ride the shuttle to Lawanda", null),
        new("FROMITZ", ["SHUTTLE", "FLOYD"], false, "get the fromitz board (via Floyd)",
            _ => Repository.GetItem<Floyd>().HasGottenTheFromitzBoard),
        new("DEFENSE_FIX", ["FROMITZ"], true, "repair meteor defense",
            _ => Repository.GetLocation<SystemsMonitors>().PlanetaryDefenseFixed),
        new("BEDISTOR_FUSED", ["SHUTTLE"], false, "find the fused bedistor", null),
        new("PLIERS", ["SHUTTLE"], false, "get the pliers", null),
        new("COURSE_FIX", ["BEDISTOR_FUSED", "PLIERS"], true, "repair course control",
            _ => Repository.GetLocation<SystemsMonitors>().CourseControlFixed),
        new("LASER", ["SHUTTLE"], false, "arm the laser (fresh battery)", null),
        // Floyd can only die at the bio lab, so his death implies this node (and, via back-fill, the
        // whole chain up to it) is done — keeps late-game state coherent even from sparse flags.
        new("BIOLOCK", ["SHUTTLE", "FLOYD"], false, "the bio lab (Floyd's sacrifice)",
            _ => Repository.GetItem<Floyd>().HasDied),
        new("MINI_CARD", ["BIOLOCK"], false, "get the miniaturization card", null),
        new("COMPUTER_FIX", ["MINI_CARD", "LASER"], false, "cure the Disease (laser the microbe)",
            _ => Repository.GetItem<Relay>().SpeckDestroyed),
        new("GAS_MASK", ["COMPUTER_FIX"], false, "clear the lab office (gas mask)", null),
        new("MUTANT_CHASE", ["GAS_MASK"], false, "escape the mutants to the cryo-elevator", null),
        new("ENDING", ["MUTANT_CHASE"], false, "the revival / ending", null)
    };

    /// <summary>Renders a compact, accurate description of the player's situation for LLM 1.</summary>
    public string Describe(IContext state)
    {
        var status = MapStatus(state);
        var done = Defs.Where(n => status[n.Id] == NodeStatus.Done).Select(n => n.Title).ToList();
        var optional = Defs.Where(n => status[n.Id] == NodeStatus.Available && n.Optional).Select(n => n.Title).ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"Location: {state.CurrentLocation.Name}. Score: {state.Score}/80.");
        if (state is PlanetfallContext c)
        {
            sb.AppendLine($"Day {c.Day}. Health: {c.SicknessDescription} Tired: {c.Tired}. Hunger: {c.Hunger}.");
            sb.AppendLine($"Floyd: {(Repository.GetItem<Floyd>().HasDied ? "DEAD (died at the bio lab)" : Repository.GetItem<Floyd>().HasEverBeenOn ? "alive and with you" : "not yet activated")}.");
        }

        // Describe STATE only (what's done / not done) — deliberately NOT "their next step is X", which
        // poisons unrelated answers (the solver grabs it for every question). The solver derives the
        // next step from the walkthrough + this done-list only when actually asked "what do I do next".
        sb.AppendLine(done.Count > 0 ? "Already accomplished: " + string.Join("; ", done) + "." : "Nothing accomplished yet.");
        var notDone = Defs.Where(n => status[n.Id] != NodeStatus.Done && !n.Optional).Select(n => n.Title).ToList();
        if (notDone.Count > 0)
            sb.AppendLine("Still to do on the main path (in order): " + string.Join("; ", notDone) + ".");
        if (optional.Count > 0)
            sb.AppendLine("Optional repairs not yet done: " + string.Join("; ", optional) + ".");

        return sb.ToString().Trim();
    }

    private static Dictionary<string, NodeStatus> MapStatus(IContext state)
    {
        var predicateDone = Defs.Where(n => n.Done is not null && n.Done(state)).Select(n => n.Id).ToHashSet();

        // Back-fill: a node is done if any node that (transitively) depends on it is verified done.
        var done = new HashSet<string>(predicateDone);
        foreach (var def in Defs)
            if (!done.Contains(def.Id) && Descendants(def.Id).Any(predicateDone.Contains))
                done.Add(def.Id);

        return Defs.ToDictionary(d => d.Id, d =>
            done.Contains(d.Id) ? NodeStatus.Done :
            d.Prereqs.All(done.Contains) ? NodeStatus.Available : NodeStatus.Locked);
    }

    private static IEnumerable<string> Descendants(string id)
    {
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
