namespace Planetfall.Item.Lawanda.Library.Computer;

internal class MainMenu : MenuItem
{
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

    internal override string Text => """
                                          1. Histooree
                                          2. Kulcur
                                          3. Teknolojee
                                          4. Jeeografee
                                          5. Xe Prajekt
                                          6. Inturlajik Gaamz
                                      """;
}