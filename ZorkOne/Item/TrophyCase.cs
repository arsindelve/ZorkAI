using System.Text;

namespace ZorkOne.Item;

public class TrophyCase : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["case", "trophy case"];

    public override string CannotBeTakenDescription => "The trophy case is securely fastened to the wall.";

    protected override int SpaceForItems => int.MaxValue;

    public override string Name => "trophy case";

    public string ExaminationDescription => Items.Any() ? "" : "The trophy case is empty.";

    public override void Init()
    {
        // Starts Empty
    }

    /// <summary>
    ///     Returns a description of the items contained in the specified container.
    /// </summary>
    /// <param name="name">The name of the container - might be needed as part of the description</param>
    /// <returns>A string representing the items contained in the specified container.</returns>
    public override string ItemListDescription(string name)
    {
        if (!Items.Any())
            return string.Empty;

        var sb = new StringBuilder();

        sb.AppendLine("Your collection of treasures consists of:");
        Items.ForEach(s => sb.AppendLine($"      {s.InInventoryDescription}"));

        return sb.ToString();
    }
}