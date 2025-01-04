namespace Planetfall.Item.Lawanda.Library.Computer;

/// <summary>
/// Represents the main menu within the computer terminal in the library. This menu serves as the root menu
/// with no parent and multiple child menu items including History, Culture, Technology,
/// Geography, Project, and Games menus.
/// </summary>
internal class MainMenu : MenuItem
{
    internal const string MainMenuText =  """
                                               1. Histooree
                                               2. Kulcur
                                               3. Teknolojee
                                               4. Jeeografee
                                               5. Xe Prajekt
                                               6. Inturlajik Gaamz
                                           """;
    
    internal override MenuItem? Parent => null;

    internal override List<MenuItem> Children =>
    [
        new HistoryMenu(),
        new CultureMenu(),
        new TechnologyMenu(),
        new GeographyMenu(),
        new ProjectMenu(),
        new GamesMenu()
    ];

    internal override string Text => MainMenuText;
}