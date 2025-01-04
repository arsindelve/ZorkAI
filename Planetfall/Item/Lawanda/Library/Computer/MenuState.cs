using JetBrains.Annotations;

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

    [UsedImplicitly] public MenuItem CurrentItem { get; set; } = new MainMenu();

    /// <summary>
    /// Navigates to the parent menu item if it exists, otherwise indicates that the action is invalid.
    /// </summary>
    /// <returns>Returns a message indicating the result of the navigation or invalid action.</returns>
    internal string GoUp()
    {
        if (CurrentItem.Parent is null)
            return
                NoEffect;

        CurrentItem = CurrentItem.Parent;
        return $"The screen clears and a different menu appears:\n\n{CurrentItem.Text}";
    }

    /// <summary>
    /// Navigates to a child menu item based on the provided index, if valid, or indicates the action is invalid.
    /// </summary>
    /// <param name="menuItem">The index of the desired child menu item.</param>
    /// <returns>Returns a message indicating the result of the navigation or invalid action.</returns>
    internal string GoDown(int menuItem)
    {
        if (CurrentItem.Children is null)
            return
                ReachedLowestLevel;

        if (menuItem > CurrentItem.Children.Count)
            return
                NoEffect;

        CurrentItem = CurrentItem.Children[menuItem - 1];

        if (CurrentItem.Children != null)
            return $"The screen clears and a different menu appears:\n\n{CurrentItem.Text}";

        return $"The screen clears and some text appears:\n\n{CurrentItem.Text}";
    }
}