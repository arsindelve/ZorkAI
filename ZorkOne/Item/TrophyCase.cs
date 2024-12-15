using System.Text;
using GameEngine;
using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.Item;

public class TrophyCase : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["case", "trophy case"];

    public override string CannotBeTakenDescription => "The trophy case is securely fastened to the wall.";

    public override bool IsTransparent => true;

    protected override int SpaceForItems => int.MaxValue;

    public override string Name => "trophy case";

    public string ExaminationDescription => Items.Any() ? ItemListDescription("", null) : "The trophy case is empty.";

    public override void Init()
    {
        // Starts Empty
    }

    /// <summary>
    ///     Returns a description of the items contained in the specified container.
    /// </summary>
    /// <param name="name">The name of the container - might be needed as part of the description</param>
    /// <param name="location"></param>
    /// <param name="indent"></param>
    /// <returns>A string representing the items contained in the specified container.</returns>
    public override string ItemListDescription(string name, ILocation? location)
    {
        if (!Items.Any())
            return string.Empty;

        var sb = new StringBuilder();

        // Map is handled differently
        if (Items.Contains(Repository.GetItem<Map>()))
            sb.AppendLine("In the trophy case is an ancient parchment which appears to be a map.");

        sb.AppendLine("Your collection of treasures consists of:");
        Items
            .Where(s => s is not Map)
            .OrderBy(s => s.GenericDescription(location))
            .ToList()
            .ForEach(s => sb.AppendLine($"      {s.GenericDescription(location)}"));

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

        if (item is not IGivePointsWhenPlacedInTrophyCase treasure)
            return;

        context.AddPoints(treasure.NumberOfPoints);
    }

    public override void OnItemRemovedFromHere(IItem item, IContext context)
    {
        base.OnItemRemovedFromHere(item, context);

        if (item is not IGivePointsWhenPlacedInTrophyCase treasure)
            return;

        context.AddPoints(-1 * treasure.NumberOfPoints);
    }
}