namespace Planetfall.Item.Lawanda.Library.Computer;

public class MenuState
{
    // ReSharper disable once MemberCanBePrivate.Global
    public MenuItem CurrentItem { get; set; } = new MainMenu();

    internal string? GoUp()
    {
        if (CurrentItem.Parent is null)
            return
                "The terminal feeps, and a message briefly appears on the screen explaining that typing that character has no meaning at the moment. ";

        CurrentItem = CurrentItem.Parent;
        return $"The screen clears and a different menu appears:\n\n{CurrentItem.Text}";
    }

    internal string? GoDown(int menuItem)
    {
        if (CurrentItem.Children is null)
            return
                "\"Yuu hav reect xe loowist levul uv xe liibreree indeks. Pleez tiip zeeroo tuu goo tuu aa hiiyur levul. If yuu reekwiir asistins, kawl xe liibrereein.\"";

        CurrentItem = CurrentItem.Children[menuItem];

        if (CurrentItem.Children != null)
            return $"The screen clears and a different menu appears:\n\n{CurrentItem.Text}";

        return $"The screen clears and some text appears:\n\n{CurrentItem.Text}";
    }
}