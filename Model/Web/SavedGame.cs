using System.Diagnostics.CodeAnalysis;

namespace Model.Web;

[method: SetsRequiredMembers]
public record SavedGame(string Id, string Name, DateTime Date)
{
    public required string Id { get; init; } = Id;

    public required string Name { get; init; } = Name;

    public required DateTime Date { get; init; } = Date;
}