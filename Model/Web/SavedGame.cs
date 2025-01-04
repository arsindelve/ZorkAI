using System.Diagnostics.CodeAnalysis;

namespace Model.Web;

public record SavedGame
{
    [SetsRequiredMembers]
    public SavedGame(string id, string name, DateTime date)
    {
        Id = id;
        Name = name;
        Date = date;
    }

    public required string Id { get; init; }

    public required string Name { get; init; }

    public required DateTime Date { get; init; }
}