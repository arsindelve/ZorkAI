using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace GameEngine;

/// <summary>
///     Who uses each <see cref="VerbFamily" />: for every class in an assembly, the families its code
///     matches against.
///     <para>
///         A noun is owned by the object that declares it. A verb grouping is declared once, in
///         <see cref="Verbs" />, for every object that answers it - so nothing about the declaration says
///         who those objects are. What does say it is the handler: a class answers a grouping exactly when
///         its code reads it (<c>action.Match(Verbs.RaiseVerbs, ...)</c>). This finds those reads in the
///         compiled code, which is what lets the parser vouch for "raise" on the basket and the rug and
///         hand "raise the lamp" to the AI.
///     </para>
/// </summary>
public static class VerbFamilyUsage
{
    private const BindingFlags Everything = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                            BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private static readonly Dictionary<short, OpCode> OpCodesByValue = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(field => (OpCode)field.GetValue(null)!)
        .ToDictionary(code => code.Value);

    /// <summary>
    ///     Every (family field, class that reads it) pair in the assembly. A read inside a lambda, an
    ///     async method or an iterator belongs to the class the programmer wrote it in, not to the helper
    ///     class the compiler moved it to. A generic class is reported as its generic definition.
    /// </summary>
    public static IReadOnlyList<(FieldInfo Family, Type UsedBy)> In(Assembly assembly)
    {
        var uses = new HashSet<(FieldInfo, Type)>();

        foreach (var type in LoadableTypes(assembly))
        {
            var writtenIn = ClassTheCodeWasWrittenIn(type);

            foreach (var method in type.GetMethods(Everything).Cast<MethodBase>().Concat(type.GetConstructors(Everything)))
            foreach (var family in FamiliesReadBy(method))
                uses.Add((family, writtenIn));
        }

        return uses.ToList();
    }

    /// <summary>
    ///     Whether <paramref name="candidate" /> is <paramref name="user" /> or derives from it, where
    ///     either may be a generic class: a handler written on <c>SlotBase&lt;TCard, TSlot&gt;</c> answers for
    ///     every concrete slot.
    /// </summary>
    public static bool IsOrDerivesFrom(Type candidate, Type user)
    {
        var wanted = Definition(user);

        for (var current = candidate; current is not null; current = current.BaseType)
            if (Definition(current) == wanted)
                return true;

        return false;
    }

    private static Type Definition(Type type)
    {
        return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
    }

    private static Type ClassTheCodeWasWrittenIn(Type type)
    {
        // Lambdas, async methods and iterators compile into nested classes the compiler names with a '<'.
        while (type.DeclaringType is not null &&
               (type.Name.Contains('<') || type.IsDefined(typeof(CompilerGeneratedAttribute), false)))
            type = type.DeclaringType;

        return Definition(type);
    }

    private static IEnumerable<FieldInfo> FamiliesReadBy(MethodBase method)
    {
        byte[]? il;
        try
        {
            il = method.GetMethodBody()?.GetILAsByteArray();
        }
        catch
        {
            // No readable body (a runtime that strips IL, an unusual method): it contributes no uses, and
            // the parser simply does not vouch for those verbs - the AI still handles them.
            yield break;
        }

        if (il is null)
            yield break;

        var typeArguments = method.DeclaringType is { IsGenericType: true } declaring
            ? declaring.GetGenericArguments()
            : null;
        var methodArguments = method is MethodInfo { IsGenericMethod: true } ? method.GetGenericArguments() : null;

        var position = 0;
        while (position < il.Length)
        {
            // Walk instruction by instruction, so that an operand which happens to contain the "load a
            // static field" byte is never mistaken for the instruction.
            if (!OpCodesByValue.TryGetValue(
                    il[position] == 0xFE && position + 1 < il.Length
                        ? unchecked((short)(0xFE00 | il[position + 1]))
                        : il[position], out var code))
                yield break;

            position += code.Size;

            if (code == OpCodes.Ldsfld || code == OpCodes.Ldsflda)
            {
                FieldInfo? field = null;
                try
                {
                    field = method.Module.ResolveField(BitConverter.ToInt32(il, position), typeArguments,
                        methodArguments);
                }
                catch
                {
                    // An unresolvable token is not a verb family this code can be said to use.
                }

                if (field is { IsStatic: true } && field.FieldType == typeof(VerbFamily))
                    yield return field;
            }

            position += code.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, position),
                _ => 4
            };
        }
    }

    private static IEnumerable<Type> LoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException partial)
        {
            return partial.Types.Where(type => type is not null)!;
        }
    }
}
