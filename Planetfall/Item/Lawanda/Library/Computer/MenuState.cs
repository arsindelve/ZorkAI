namespace Planetfall.Item.Lawanda.Library.Computer;

/// <summary>
/// Represents the state and navigation of a menu system within the computer terminal.
/// </summary>
public class MenuState
{
    internal const string NoEffect =
        "The terminal feeps, and a message briefly appears on the screen explaining that typing that character has no meaning at the moment.";

    internal const string ReachedLowestLevel =
        "\"Yuu hav reect xe loowist levul uv xe liibreree indeks. Pleez tiip zeeroo tuu goo tuu aa hiiyur levul. If yuu reekwiir asistins, kawl xe liibrereein.\"";

    // Breadcrumb path: 1-based child indices from the root. Empty = at MainMenu.
    // Storing the path (not the MenuItem object) is what survives JSON serialization —
    // MenuItem.Parent is internal so Newtonsoft.Json skips it, making CurrentItem.Parent
    // null after a round-trip and breaking GoUp() (issue #323).
    [UsedImplicitly] public List<int> Path { get; set; } = [];

    // Cached result of walking Path from the root. Invalidated whenever Path mutates.
    [Newtonsoft.Json.JsonIgnore] private MenuItem? _current;

    // Computed from Path; cached so repeated accesses within one method call don't
    // re-allocate a fresh child list at every level.
    [Newtonsoft.Json.JsonIgnore]
    public MenuItem CurrentItem => _current ??= BuildCurrentItem();

    private MenuItem BuildCurrentItem()
    {
        MenuItem current = new MainMenu();
        foreach (var index in Path)
        {
            var children = current.Children;
            if (children is null || index < 1 || index > children.Count)
                return current;
            current = children[index - 1];
        }
        return current;
    }

    /// <summary>
    /// Navigates to the parent menu item if it exists, otherwise indicates that the action is invalid.
    /// </summary>
    internal string GoUp()
    {
        if (Path.Count == 0)
            return NoEffect;

        Path.RemoveAt(Path.Count - 1);
        _current = null;
        return $"The screen clears and a different menu appears:\n\n{CurrentItem.Text}";
    }

    /// <summary>
    /// Navigates to a child menu item based on the provided index, if valid, or indicates the action is invalid.
    /// </summary>
    internal string GoDown(int menuItem)
    {
        var children = CurrentItem.Children;

        if (children is null)
            return ReachedLowestLevel;

        // menuItem < 1 is currently unreachable (ProcessKeyPress routes 0 → GoUp),
        // but guards against unexpected future call sites.
        if (menuItem < 1 || menuItem > children.Count)
            return NoEffect;

        Path.Add(menuItem);
        _current = null;
        var current = CurrentItem;

        if (current.Children != null)
            return $"The screen clears and a different menu appears:\n\n{current.Text}";

        return $"The screen clears and some text appears:\n\n{current.Text}";
    }
}
