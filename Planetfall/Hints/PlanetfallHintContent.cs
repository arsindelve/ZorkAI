using GameEngine;
using GameEngine.Hints;
using Model.Hints;
using Model.Interface;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Hints;

/// <summary>
///     The authored Planetfall puzzle-hint ladders (Docs/hints/planetfall/06). Each node has 3 rungs:
///     vague nudge → approach → exact solution. The engine reveals one at a time.
/// </summary>
internal sealed class PlanetfallCorpus : IHintCorpus
{
    private static readonly Dictionary<string, string[]> Ladders = new()
    {
        ["ESCAPE_POD"] = ["The ship is coming apart; standing here won't save you.", "There's an escape pod to port — get in and strap in.", "port, then sit, then wait out the descent."],
        ["LAND"] = ["You've landed underwater; grab what's useful before leaving.", "Take the survival kit, open the bulkhead, climb out of the water.", "take kit, open door, out, then up toward the complex."],
        ["MAGNET"] = ["You'll want a tool that grabs metal before long.", "There's a magnet in the Tool Room.", "In the Tool Room: take magnet."],
        ["FLOYD"] = ["One of these robots is more than scrap — and you shouldn't do this alone.", "Activate the multipurpose robot and give him a moment.", "In the Robot Shop: activate floyd, then wait until he comes to life."],
        ["STEEL_KEY"] = ["There's a crevice here with something small and metal out of reach.", "Use the magnet on the crevice.", "put magnet on crevice — a steel key falls out."],
        ["STORAGE_WEST"] = ["A padlocked door blocks a storeroom you'll need.", "Unlock the padlock with the steel key.", "unlock padlock with key, remove lock, open door, north."],
        ["LADDER"] = ["You'll need a way across a gap later.", "The storeroom holds a ladder.", "take ladder."],
        ["CROSS_RIFT"] = ["A rift cuts the corridor; you can't jump it, but you carry the answer.", "The ladder extends — lay it across the rift.", "drop ladder, extend ladder, place ladder across rift, then cross."],
        ["UPPER_CARD"] = ["The offices across the rift hold access cards.", "Open the small office desk.", "open desk, take upper card."],
        ["KITCHEN_CARD"] = ["The complex runs on access cards.", "The small office desk holds the kitchen card too.", "take kitchen card."],
        ["SHUTTLE_CARD"] = ["You'll need to reach the far complex eventually.", "The large office desk holds the shuttle card.", "open desk, take shuttle card."],
        ["KITCHEN"] = ["The kitchen is locked behind a card slot.", "Slide the kitchen card through the slot.", "slide kitchen card through slot, then south. (Grab the canteen in the Mess Hall.)"],
        ["LOWER_CARD"] = ["The kitchen holds the last elevator card.", "Take the lower-elevator card inside.", "take lower card."],
        ["FLASK"] = ["You'll need to carry a fluid up to the tower.", "Find a flask to carry it.", "In the Tool Room: take flask."],
        ["FILL_FLASK_A"] = ["You need to fill the flask somewhere.", "Use the spout in the Machine Shop.", "put flask under spout, press black button, take flask."],
        ["OPEN_ELEVATOR"] = ["The upper elevator won't open until you prime it.", "Press the lobby buttons in order and wait.", "press blue button, press red button, wait until it opens."],
        ["TOWER_UP"] = ["The card runs the elevator once it's open.", "Slide the upper card and press up.", "slide upper access card through slot, press up button, wait."],
        ["COMM_FIX"] = ["The tower has holes lit by colored lights; the right fluid does something.", "Pour fluid in the black-lit hole, refill with the gray button, pour again.", "NE, pour fluid into hole; refill via the gray button; back up, NE, pour fluid into hole."],
        ["LOWER_ELEVATOR"] = ["The shuttle is reached from below.", "Take the lower elevator down with its card.", "slide lower access card through slot, press down button."],
        ["SHUTTLE"] = ["The other half of the game is across the mountains.", "Board the shuttle, activate it with the shuttle card, drive it with the lever.", "slide shuttle access card through slot, push lever, pull lever, wait, then pull lever to stop."],
        ["FROMITZ"] = ["A wall panel is flashing a malfunction; a part is one room over, behind Floyd.", "Have Floyd fetch the shiny fromitz board.", "In the Repair Room: floyd, take board."],
        ["DEFENSE_FIX"] = ["The defense panel needs its burnt-out part replaced.", "Remove the fried board, fit the shiny one.", "open panel, take second (the fried one), put shiny in panel."],
        ["BEDISTOR_FUSED"] = ["The course-control cube has a fused component.", "Open the cube to see the problem.", "open cube — the bedistor is fused."],
        ["PLIERS"] = ["You can't pull the fused part by hand.", "Get the pliers from the Tool Room.", "take pliers."],
        ["COURSE_FIX"] = ["Replace the fused bedistor with a working one.", "Pull the fused one with pliers, fit a good one.", "take fused with pliers, put good in cube."],
        ["TELEPORT_CARD"] = ["Something here shortcuts the long trips.", "A hidden pocket in Lab Storage holds a card.", "open pocket, take teleportation."],
        ["LASER"] = ["There's a laser, but its battery is dead.", "Swap in the fresh battery from Lab Storage.", "take laser, remove battery, drop battery; later: take fresh battery, put battery in laser."],
        ["BIOLOCK"] = ["The card you need is behind a door, in a lab full of deadly mutations.", "You can't survive going in — but your companion volunteers.", "Work the bio-lock doors so Floyd dashes in for the card: open door, close door, wait, open door, close door."],
        ["MINI_CARD"] = ["Floyd retrieved something before he died.", "Take the miniaturization card he recovered.", "take miniaturization card."],
        ["COMPUTER_FIX"] = ["The real fault is microscopic — a damaged microbe in the computer.", "Miniaturize, set the laser low, and destroy the speck.", "slide mini card through slot, type 384, set laser to 1, shoot speck with laser (twice)."],
        ["GAS_MASK"] = ["A memo warns about an emergency system you can't breathe.", "Wear the gas mask before triggering it.", "read memo, take mask, wear gas mask, press red button, open door."],
        ["MUTANT_CHASE"] = ["The stunned mutations are recovering and chasing you.", "Run for the cryo-elevator and seal it.", "flee west/south to the Cryo-Elevator, then press button."]
    };

    public bool TryGetLadder(string nodeId, out RungLadder ladder)
    {
        if (Ladders.TryGetValue(nodeId, out var rungs))
        {
            ladder = new RungLadder(nodeId, rungs);
            return true;
        }

        ladder = null!;
        return false;
    }
}

/// <summary>
///     Planetfall lore source (Docs/hints/planetfall/05), held in context (the corpus is small) and
///     gated by a coarse spoiler tier derived from progress: the deep backstory only unlocks once the
///     player has reached the Lawanda half (where the library is).
/// </summary>
internal sealed class PlanetfallLoreSource : ILoreSource
{
    private const string Observable =
        "You are a lowly Ensign Seventh Class of the Stellar Patrol, shipwrecked after the S.P.S. Feinstein " +
        "exploded. You've landed on a planet whose vast complex is fully automated and utterly deserted.";

    private const string Investigated =
        Observable + " The planet is Resida. Its entire population was cryogenically frozen to wait out a " +
        "plague — The Disease — which escaped from a cryogenic-research center. The automated Kalamontee and " +
        "Lawanda complexes were built to watch over the sleepers and search for a cure, but the automation has " +
        "broken down. The planetary systems (Course Control, Defense, Project Control) keep the world habitable.";

    public Task<LoreAnswer> Answer(string question, IContext liveState, ProgressState progress, IHintLanguageModel llm)
    {
        // Tier: once anything in the Lawanda half is reachable (SHUTTLE done), the player has had access
        // to the library — reveal the full backstory; otherwise keep it to what's observable.
        var investigated = progress.Nodes.GetValueOrDefault("SHUTTLE") == NodeStatus.Done;
        var grounded = investigated ? Investigated : Observable;
        return llm.PhraseLore(question, grounded, HintPersonas.SnarkyNarrator)
            .ContinueWith(t => new LoreAnswer(true, t.Result));
    }
}

/// <summary>
///     Answers "why am I sick / tired / hungry?" from the live survival-clock state. Returns
///     Grounded=false for questions it doesn't recognise (the engine then falls back to lore).
/// </summary>
internal sealed class PlanetfallMechanicExplainer : IMechanicExplainer
{
    public Task<LoreAnswer> Explain(string question, IContext liveState, IHintLanguageModel llm)
    {
        var q = question.ToLowerInvariant();
        if (liveState is not PlanetfallContext ctx)
            return Task.FromResult(new LoreAnswer(false, ""));

        string? source = null;
        if (q.Contains("sick") || q.Contains("disease") || q.Contains("ill") || q.Contains("fever"))
            source = $"You've contracted The Disease — the same plague that emptied this planet. It is day {ctx.Day} " +
                     "and it worsens over time; experimental medicine slows it, but the real cure is in the lab. Don't dawdle.";
        else if (q.Contains("tired") || q.Contains("sleep") || q.Contains("weary"))
            source = "You're running low on sleep. Find a safe place to rest before you collapse — a bunk, not a corridor.";
        else if (q.Contains("hungry") || q.Contains("thirst") || q.Contains("eat") || q.Contains("food"))
            source = "You're getting hungry and thirsty. There are rations to eat and water for the canteen — see to it.";

        return source is null
            ? Task.FromResult(new LoreAnswer(false, ""))
            : llm.PhraseLore(question, source, HintPersonas.SnarkyNarrator).ContinueWith(t => new LoreAnswer(true, t.Result));
    }
}

/// <summary>Planetfall soft-lock rules and proactive survival nudges (Docs/hints/planetfall/03 + 01 §survival).</summary>
internal static class PlanetfallRules
{
    public static readonly IReadOnlyList<ISoftLockRule> SoftLocks = new ISoftLockRule[]
    {
        new DiseaseClockRule()
    };

    public static readonly IReadOnlyList<IProactiveRule> Proactive = new IProactiveRule[]
    {
        new TiredRule(),
        new HungerRule(),
        new DiseaseRule()
    };

    // The Disease is a timed pressure, not a hard lock — surface it as an escalating caution while the
    // cure (COMPUTER_FIX / Relay.SpeckDestroyed) is incomplete.
    private sealed class DiseaseClockRule : ISoftLockRule
    {
        public SoftLockVerdict Evaluate(IContext liveState, ProgressState progress)
        {
            if (liveState is not PlanetfallContext ctx) return SoftLockVerdict.None;
            if (Repository.GetItem<Relay>().SpeckDestroyed) return SoftLockVerdict.None;
            return ctx.Day >= 6
                ? new SoftLockVerdict(SoftLockKind.Warning,
                    "The Disease is well advanced — time is short. Make the lab and the cure your priority.")
                : SoftLockVerdict.None;
        }
    }

    private sealed class TiredRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext liveState) =>
            liveState is PlanetfallContext ctx && (int)ctx.Tired >= 1
                ? new ProactiveNudge("sleep", "You're getting tired — find a safe place to sleep.", 3)
                : null;
    }

    private sealed class HungerRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext liveState) =>
            liveState is PlanetfallContext ctx && (int)ctx.Hunger >= 1
                ? new ProactiveNudge("hunger", "You're getting hungry and thirsty — find food and water.", 3)
                : null;
    }

    private sealed class DiseaseRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext liveState) =>
            liveState is PlanetfallContext ctx && ctx.Day >= 4 && !Repository.GetItem<Relay>().SpeckDestroyed
                ? new ProactiveNudge("disease", "You're getting sicker by the day — the cure is in the lab.", 5)
                : null;
    }
}
