using System.Text;

namespace ZorkOne.Item;

public class TrophyCase : OpenAndCloseContainerBase, ICanBeExamined
{
    // This is a list of every item that's ever been placed here. 
    // We track this so you don't get double points for putting something 
    // in the case, taking it out and putting it back again. 
    public List<IItem> ItemsPlacedInside = [];

    public override string[] NounsForMatching => ["case", "trophy case"];

    public override string CannotBeTakenDescription => "The trophy case is securely fastened to the wall.";

    public override bool IsTransparent => true;

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

    /// <summary>
    ///     Handles the action that takes place when an item is placed inside the TrophyCase.
    /// </summary>
    /// <param name="item">The item being placed inside the TrophyCase.</param>
    /// <param name="context">The current game context.</param>
    public override void OnItemPlacedHere(IItem item, IContext context)
    {
        base.OnItemPlacedHere(item, context);

        if (item is not IGivePointsWhenPlacedInTrophyCase treasure) return;
        if (ItemsPlacedInside.Contains(item)) return;

        context.AddPoints(treasure.NumberOfPoints);
        ItemsPlacedInside.Add(item);
    }

    // TODO: Score goes down if you take the item out!!!
}