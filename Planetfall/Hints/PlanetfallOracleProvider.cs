using System.Text;
using GameEngine;
using GameEngine.Hints.Data;
using GameEngine.Hints.Oracle;
using Model.Hints;
using Model.Interface;
using Newtonsoft.Json;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Hints;

/// <summary>
///     Planetfall for the hint oracle: the narrator's voice, the game bible (Hints/Oracle/planetfall-bible.md,
///     written offline by Planetfall.Tests/Hints/Oracle/GameBibleGenerator from the source, a real playthrough
///     and the invisiclues), and a factual description of the player's situation. Nothing here knows any
///     puzzle: the situation is read mechanically — vitals, inventory, rooms visited, and whatever in the world
///     is no longer as it was when the game began.
/// </summary>
public sealed class PlanetfallOracleProvider : IOracleProvider
{
    private static readonly Lazy<string> Bible = new(() => ReadResource("planetfall-bible.md") ?? string.Empty);

    private static readonly Lazy<IReadOnlyDictionary<string, string>> Baseline = new(() =>
    {
        var json = ReadResource("planetfall-initial-state.json");
        return json is null
            ? new Dictionary<string, string>()
            : JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
    });

    public static bool IsAvailable => Bible.Value.Length > 0;

    public HintPersona Persona => new(
        "You are the invisible narrator of the Infocom game Planetfall: dry, warm, a little wry, and quietly on " +
        "the player's side. A joke may decorate an answer, but it never replaces one. The voice at its best, as a " +
        "register to aim for and not a line to reuse — asked how to get past the troll in Zork: \"Wrong adventure, " +
        "I'm afraid; this is Resida, not the Great Underground Empire. I can help with the rather more immediate " +
        "problem of getting off this planet alive.\"",
        "Planetfall");

    public string GameKnowledge => Bible.Value;

    public string DescribeSituation(IContext state)
    {
        var now = StateFlattener.Flatten(state);
        var sb = new StringBuilder();

        sb.AppendLine($"Location: {state.CurrentLocation?.Name ?? "unknown"}");
        sb.AppendLine($"Score: {state.Score}   Moves: {state.Moves}");
        if (state is PlanetfallContext pc)
            sb.AppendLine($"Day {pc.Day}, time {pc.CurrentTime}.  Health: {pc.SicknessDescription} (sickness level {pc.SicknessCounter} of 8+ = death).  Hunger: {pc.Hunger}.  Tiredness: {pc.Tired}.");

        var floyd = Repository.GetItem<Floyd>();
        sb.AppendLine("Floyd: " + (floyd.HasDied ? "dead"
            : !floyd.HasEverBeenOn ? "never switched on (the player may not know he exists)"
            : floyd.CurrentLocation == state.CurrentLocation ? $"alive, switched {(floyd.IsOn ? "on" : "OFF")}, here with the player"
            : $"alive, switched {(floyd.IsOn ? "on" : "OFF")}, NOT with the player — he is in: {floyd.CurrentLocation?.Name ?? "somewhere unknown"}"));

        var carried = state.Items ?? [];
        sb.AppendLine("Carrying: " + (carried.Count == 0 ? "nothing" : string.Join(", ", carried.Select(Describe))));

        var rooms = WorldDelta.VisitedRooms(now);
        sb.AppendLine($"Rooms they have been in ({rooms.Count}): {string.Join(", ", rooms)}");

        // The conference-room combination is rolled lazily, the first time anything reads it - and this read-only
        // path never saves, so a number seen here may not be the number the game later settles on. It is not a
        // fact about the world yet; the narrator sends players to the note instead.
        var delta = WorldDelta.Describe(Baseline.Value, now).Where(line => !line.Contains("UnlockCode")).ToList();
        if (delta.Count > 0)
        {
            sb.AppendLine("What is different in the world from when the game began (engine state, object.property = value):");
            foreach (var line in delta) sb.AppendLine("  " + line);
        }

        return sb.ToString();
    }

    private static string Describe(Model.Item.IItem item)
    {
        var inside = item is ICanContainItems { Items.Count: > 0 } container
            ? $" (holding: {string.Join(", ", container.Items.Select(i => i.Name))})"
            : "";
        return item.Name + inside;
    }

    private static string? ReadResource(string fileName)
    {
        var asm = typeof(PlanetfallOracleProvider).Assembly;
        var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
        if (name is null) return null;
        using var stream = asm.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
