using System.Reflection;

namespace UnitTests.Parser;

/// <summary>
///     The generated files the parser tests read (the walkthrough interactions, each game's interaction
///     set) are compiled into the test assembly that owns them, as the intent mappings are, so the tests
///     find them wherever the assembly is run from.
/// </summary>
public static class EmbeddedFile
{
    /// <summary>The file's text, or null if the assembly holds no file of that name.</summary>
    public static string? Read(Assembly assembly, string fileName)
    {
        var resource = assembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase));

        if (resource is null)
            return null;

        using var reader = new StreamReader(assembly.GetManifestResourceStream(resource)!);
        return reader.ReadToEnd();
    }
}
