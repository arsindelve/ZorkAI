using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Model.Interface;
using Model.Item;
using Model.Location;
using Newtonsoft.Json;
using NUnit.Framework;

namespace UnitTests;

/// <summary>
///     Systemic guard for the recurring stateless-serialization trap behind issues #365 and #493.
///
///     The deployed game is stateless: every turn re-hydrates the whole game from the base64 session, and
///     <c>GameEngine.DoNotSerializeReadOnlyPropertiesResolver</c> deliberately drops every property that is
///     not writable. A get-only auto-property whose value is a <em>mutable reference type</em> (a state
///     machine, a manager, a mutable collection) is therefore never written to the session and silently
///     re-initialises to its <c>= new()</c> default on every request — so any per-turn/session state it
///     holds resets every turn in production, while every single-process test keeps passing.
///
///     <c>BioLockEast.StateMachine</c> was exactly this shape and made Planetfall unwinnable in prod for
///     two separate issues before it was given a setter. This test reflects over every serialized type
///     (locations, items, contexts) and fails if any such get-only-holding-mutable-state property exists,
///     so the next one is caught at PR time instead of in production. The fix is almost always to add a
///     setter (<c>{ get; set; }</c> or <c>{ get; private set; }</c>) so the object round-trips; genuinely
///     immutable, always-reconstructed config goes on <see cref="Allowlisted"/> with a reason.
///
///     Scope and known limitations (kept deliberately narrow to stay false-positive-free):
///     - It scans the serialization roots — every concrete location, item, and context in every game
///       assembly (discovered dynamically, so new games are covered automatically) — plus their inherited
///       base properties.
///     - It does NOT recurse into nested state-holders reached via writable properties (e.g.
///       <c>BioLockStateMachineManager</c>, timers, coordinators). Those must likewise keep all of their
///       own state on writable properties; today they all do, but nothing here enforces it.
///     - It only recognises the auto-property shape (a compiler-generated backing field). The
///       semantically identical field-backed form —
///       <c>private readonly Mgr _m = new(); public Mgr M => _m;</c> — also loses state (a private field
///       is not serialized either) but is not detected, because a read-only expression-bodied property
///       cannot be told apart from a genuinely computed one by reflection alone. Prefer a writable
///       property for any serialized state; do not hide it behind a read-only field accessor.
/// </summary>
[TestFixture]
public class SessionStateSerializationGuardTests
{
    // A floor of games that discovery must always find. This is a failsafe against discovery silently
    // finding nothing (which would make the whole guard pass vacuously) - NOT the list of what gets
    // scanned. Any game whose DLL is in the test output is discovered and scanned automatically, so a
    // newly-added game is covered without touching this list; it only needs updating if a game is renamed
    // or removed.
    private static readonly string[] ExpectedGames = ["ZorkOne", "Planetfall", "EscapeRoom", "ZorkTwo"];

    /// <summary>
    ///     "DeclaringType.PropertyName" entries that are get-only reference-typed properties on serialized
    ///     types but are provably safe because their value is immutable configuration that is reconstructed
    ///     identically on every fresh instance (never mutated after construction), so losing it to the
    ///     serializer and rebuilding it from the initializer is a no-op. Each entry must carry a reason.
    /// </summary>
    private static readonly HashSet<string> Allowlisted = new();

    [Test]
    public void NoSerializedType_HasAGetOnlyPropertyHoldingMutableState()
    {
        var gameAssemblies = DiscoverGameAssemblies();

        // Failsafe: if discovery finds fewer games than expected, the guard would be silently protecting
        // less than it claims - fail loudly rather than pass vacuously.
        gameAssemblies.Select(a => a.GetName().Name!).Should().Contain(ExpectedGames,
            "the serialization guard must scan every game; if discovery misses one it is not protecting it");

        var offenders = new List<string>();
        var seen = new HashSet<string>();

        foreach (var assembly in gameAssemblies)
        foreach (var type in GetLoadableTypes(assembly))
        {
            if (!IsSerializedType(type))
                continue;

            // Public instance properties, INCLUDING inherited ones, so get-only state declared on an
            // abstract base (ContainerBase, LocationBase, Context<T>) is covered too. Newtonsoft's default
            // contract resolver serializes public members, so those are the ones the trap applies to;
            // `seen` dedups each declaring-type property so a base property is only reported once.
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!IsSilentStateLossRisk(prop))
                    continue;

                var key = $"{prop.DeclaringType!.FullName}.{prop.Name}";
                if (!seen.Add(key) || Allowlisted.Contains($"{prop.DeclaringType!.Name}.{prop.Name}"))
                    continue;

                offenders.Add($"{prop.DeclaringType!.Name}.{prop.Name} : {FriendlyTypeName(prop.PropertyType)}");
            }
        }

        offenders.Should().BeEmpty(BuildFailureMessage(offenders));
    }

    private static IReadOnlyList<Assembly> DiscoverGameAssemblies()
    {
        // Directory-scan the test output instead of hardcoding game names, so a newly-added game is
        // covered automatically once it is referenced (its DLL then lands here). A game assembly is one
        // that references GameEngine and defines at least one concrete serialized type. GameEngine itself
        // is intentionally excluded: its abstract bases (LocationBase, ItemBase, Context<T>) carry no
        // concrete types, and their inherited properties are already covered when a concrete game type
        // that derives from them is scanned.
        var assemblies = new List<Assembly>();

        foreach (var dll in Directory.GetFiles(AppContext.BaseDirectory, "*.dll"))
        {
            Assembly asm;
            try
            {
                asm = Assembly.LoadFrom(dll);
            }
            catch
            {
                continue; // unmanaged / unloadable dll in the output - not a game assembly
            }

            if (!asm.GetReferencedAssemblies().Any(r => r.Name == "GameEngine"))
                continue;

            if (GetLoadableTypes(asm).Any(IsSerializedType))
                assemblies.Add(asm);
        }

        return assemblies;
    }

    private static bool IsSerializedType(Type type)
    {
        if (!type.IsClass || type.IsAbstract)
            return false;

        return typeof(ILocation).IsAssignableFrom(type) ||
               typeof(IItem).IsAssignableFrom(type) ||
               typeof(IContext).IsAssignableFrom(type);
    }

    private static bool IsSilentStateLossRisk(PropertyInfo prop)
    {
        // Serializable, writable properties round-trip fine. The resolver only drops properties with no
        // setter at all (a private/protected setter still counts as Writable to Newtonsoft, so those are
        // safe). [JsonIgnore] properties are intentionally excluded from serialization, so resetting them
        // is by design.
        if (prop.GetMethod is null || prop.SetMethod is not null)
            return false;

        if (prop.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
            return false;

        // Only auto-properties hold state. Expression-bodied read-only properties (=> ...) recompute on
        // every access, so the serializer dropping them is harmless. An auto-property has a
        // compiler-generated "<Name>k__BackingField".
        if (!IsAutoProperty(prop))
            return false;

        // A get-only auto-property of a value type / string / enum can never diverge from its initializer
        // (nothing can reassign it without a setter), so it cannot lose state. Only a mutable reference
        // type whose contents are mutated in place is dangerous.
        var t = prop.PropertyType;
        return t is { IsValueType: false } && t != typeof(string);
    }

    private static bool IsAutoProperty(PropertyInfo prop)
    {
        return prop.DeclaringType!.GetField($"<{prop.Name}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) is not null;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }

    private static string FriendlyTypeName(Type t)
    {
        if (!t.IsGenericType)
            return t.Name;

        var args = string.Join(", ", t.GetGenericArguments().Select(FriendlyTypeName));
        return $"{t.Name[..t.Name.IndexOf('`')]}<{args}>";
    }

    private static string BuildFailureMessage(IReadOnlyCollection<string> offenders)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "these get-only auto-properties hold a mutable reference type on a serialized type, so the " +
            "stateless session serializer (DoNotSerializeReadOnlyPropertiesResolver) drops them and they " +
            "reset to a fresh instance every turn in production (the issue #365/#493 trap). Give each a " +
            "setter so it round-trips, or add it to Allowlisted with a reason if it is immutable config:");
        foreach (var offender in offenders.OrderBy(x => x))
            sb.AppendLine($"  - {offender}");
        return sb.ToString();
    }
}
