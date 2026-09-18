using System.Reflection;
using System.Text;
using GameEngine;
using GameEngine.Hints;
using Model.Interface;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.Library.Computer;

namespace Planetfall.Hints;

/// <summary>
///     Planetfall's lore source: the backstory digest from Docs/hints/planetfall/05, tiered by what the
///     player has discovered, plus the official invisiclues — section by section, unlocked as the player
///     reaches each area of the game. The model answering a lore question receives only this text, so a
///     fact from a later tier or a later area is not in its hands to reveal.
/// </summary>
internal sealed class PlanetfallLoreSource : ILoreSource
{
    // ---- the digest (05), by tier ---------------------------------------------------------------

    private const string Observable =
        "You are an Ensign Seventh Class in the Stellar Patrol, serving aboard the S.P.S. Feinstein under a petty " +
        "martinet, Ensign First Class Blather, who had you scrubbing Deck Nine. A massive explosion tore the " +
        "Feinstein apart while you were swabbing the deck; you reached an escape pod and rode it down to the " +
        "surface of a planet, landing underwater near a large automated complex. The complex is fully automated " +
        "and utterly deserted: machines run, doors open, but there are no people anywhere. Floyd is a childlike, " +
        "enthusiastic multipurpose B-19-series robot who can be reactivated in the Robot Shop and becomes your " +
        "loyal companion. The alien Ambassador aboard the Feinstein was there to liven up the opening and nothing " +
        "more. Why the ship exploded is not something you can know yet; that comes much later.";

    private const string Environmental =
        "Your family has served the Patrol for five generations (your great-great-grandfather was a High " +
        "Admiral and a founding father of the Patrol); you grew up on Gallium, and this was the third of your " +
        "four tours. The Feinstein had been sent to investigate a remote planetary system that archaeologists " +
        "thought might once have been part of the Second Union. Since arriving you have been getting sick: you " +
        "have contracted The Disease, a plague, and it worsens over the days — feverish, then somewhat sick, then " +
        "very sick, then near death. It will not stop on its own. A bottle of experimental disease-suppression " +
        "medicine exists that buys time; it is not a cure.";

    private const string Investigated =
        "The planet is Resida. Its entire population was cryogenically frozen and lies in stasis, waiting out a " +
        "plague — The Disease — which was accidentally released from the Center for Advanced Cryogenic Research, " +
        "whose work on extending the cryogenic period succeeded even as the plague escaped. The two automated " +
        "complexes, Kalamontee and Lawanda, exist solely to monitor the sleepers and run an automated search for " +
        "a cure; the Project ran in four phases (build the complexes; freeze the population; automated " +
        "monitoring and cure research; revival and inoculation once a cure is found). The automation has been " +
        "running unattended for a very long time and has broken down. Three planetary systems keep Resida " +
        "habitable and are failing: Course Control (climate and orbit), Planetary Defense (an automatic meteor " +
        "defense whose discrimination circuit has failed — which is very likely what shot the Feinstein down, " +
        "mistaking it for a meteor), and Project Control. Resida's people descend, legend says, from the Second " +
        "Union; a high civilization collapsed into the centuries-long Great Hiatus before the New Technocracy " +
        "restored it, and The Disease struck at that peak. Cryogenics lets doctors freeze patients until a cure " +
        "is found; robots like the B-19 series do the work that once took whole teams.";

    private const string Endgame =
        "Once the cure is in place, the automated systems revive Veldina, leader of Resida; the ending varies " +
        "with which of the three planetary systems you repaired, the best ending needing all three.";

    private const string Mechanics =
        "Survival: you must eat and drink (rations from the kitchen and dispensers; water in the canteen), you " +
        "must sleep (safely — a dorm bunk, never the infirmary bed), and The Disease worsens each day; the " +
        "experimental medicine slows it, and the real cure is the work of the Lawanda lab.";

    // ---- the invisiclues, by section -----------------------------------------------------------

    private static readonly Lazy<IReadOnlyList<(string Header, string Body)>> Invisiclues = new(LoadInvisiclues);

    /// <summary>
    ///     Section header prefix -> the DAG node the player must have completed to see it. Null means always
    ///     available. Sections not listed ("Sample Question", "How to Get All 80 Points") are never shown.
    /// </summary>
    private static readonly (string Prefix, string? Gate)[] SectionGates =
    {
        ("Aboard the Feinstein", null),
        ("The Pod Trip", null),
        ("General Questions", null),
        ("The Dormitory Area", "LAND"),
        ("The Admin/Mech Area", "LAND"),
        ("The Helicopter Trip", "LAND"),
        ("The Elevators and Tower Area", "CROSS_RIFT"),
        ("The Shuttle Trip", "SHUTTLE_CARD"),
        ("The Systems and Library Area", "SHUTTLE"),
        ("The ProjCon and Lab Area", "SHUTTLE"),
        ("The Computer", "MINI_CARD"),
        ("For Your Amusement", "COMPUTER_FIX")
    };

    public string GroundedText(IContext liveState, ProgressState progress)
    {
        var tier = TierOf(liveState, progress);

        var sb = new StringBuilder();
        sb.AppendLine("WHAT THE PLAYER COULD KNOW SO FAR:");
        sb.AppendLine(Observable);
        if (tier >= 1) sb.AppendLine(Environmental);
        if (tier >= 2) sb.AppendLine(Investigated);
        if (tier >= 3) sb.AppendLine(Endgame);
        sb.AppendLine();
        sb.AppendLine("HOW THE GAME TREATS THE PLAYER:");
        sb.AppendLine(Mechanics);
        sb.AppendLine(Condition(liveState));

        var sections = Invisiclues.Value
            .Where(s => SectionGates.Any(g =>
                s.Header.StartsWith(g.Prefix, StringComparison.OrdinalIgnoreCase) &&
                (g.Gate is null || progress.IsDone(g.Gate))))
            .ToList();

        if (sections.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("OFFICIAL HINTS FOR THE AREAS REACHED SO FAR (each answer's rungs go from vague to specific):");
            foreach (var (header, body) in sections)
            {
                sb.AppendLine();
                sb.AppendLine("## " + header);
                sb.AppendLine(body.Trim());
            }
        }

        return sb.ToString().Trim();
    }

    /// <summary>0 observable · 1 environmental (landed) · 2 investigated (the library) · 3 endgame (cured).</summary>
    internal static int TierOf(IContext liveState, ProgressState progress)
    {
        if (progress.IsDone("COMPUTER_FIX")) return 3;
        if (progress.IsDone("SHUTTLE") || LibraryUsed()) return 2;
        if (progress.IsDone("LAND")) return 1;
        return 0;
    }

    private static bool LibraryUsed()
    {
        try
        {
            return Repository.GetItem<ComputerTerminal>().IsOn;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string Condition(IContext state)
    {
        var sb = new StringBuilder();
        if (state is PlanetfallContext c)
            sb.Append($"Current condition: day {c.Day}; health: {c.SicknessDescription}; hunger: {c.Hunger}; tired: {c.Tired}. ");

        try
        {
            var floyd = Repository.GetItem<Floyd>();
            sb.Append(floyd.HasDied ? "Floyd is dead."
                : floyd.HasEverBeenOn ? "Floyd is awake and with the player."
                : "Floyd has not been activated.");
        }
        catch (Exception)
        {
            // No Floyd in this harness — nothing to report.
        }

        return sb.ToString().Trim();
    }

    private static IReadOnlyList<(string Header, string Body)> LoadInvisiclues()
    {
        var asm = typeof(PlanetfallLoreSource).Assembly;
        var name = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("invisiclues.md", StringComparison.OrdinalIgnoreCase));
        if (name is null) return Array.Empty<(string, string)>();

        using var stream = asm.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return ParseSections(reader.ReadToEnd());
    }

    /// <summary>Splits a markdown document on its "## " headers into (header, body) pairs.</summary>
    internal static IReadOnlyList<(string Header, string Body)> ParseSections(string markdown)
    {
        var sections = new List<(string, string)>();
        string? header = null;
        var body = new StringBuilder();

        foreach (var rawLine in markdown.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                if (header is not null) sections.Add((header, body.ToString()));
                header = line[3..].Trim();
                body.Clear();
            }
            else if (header is not null)
            {
                body.AppendLine(line);
            }
        }

        if (header is not null) sections.Add((header, body.ToString()));
        return sections;
    }
}
