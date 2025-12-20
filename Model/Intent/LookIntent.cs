namespace Model.Intent;

/// <summary>
/// Represents the parser's determination of the user's intent related to performing a "look" operation
/// i.e. "Look Around" or "Where am I?"
/// </summary>
public record LookIntent : IntentBase
{
    public static string TagName => "look";
}