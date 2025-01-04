namespace Planetfall.Item.Lawanda.Library.Computer;

internal class GeographyMenu : MenuItem
{
    internal const string MainMenu = """
                                         0. Maan Menyuu
                                         1. Planit Landmasiz
                                         2. Undursee Reejunz
                                         3. Spaas Kolooneez
                                     """;

    internal const string MenuOne = """
                                    "Sins xe staabulizaashun uv xe oorbit uv Resida, preesiislee 47.79 pursent uv xe planits surfis iz land. Xe land iz diviidid intuu tuu priimeree landmasiz,
                                    Andoor and Fruulik, plus siks lesur landmasiz. Xe gloobul kapitul, Pilandoor, iz on xe eesturn koost uv Andoor."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa hiiyur levul."
                                    Floyd asks if you want to play Hucka-Bucka-Beanstalk.
                                    """;

    internal const string MenuTwo = """
                                    "Xe furst undursee habutats wur oopind in 2992, and tuudaa, neerlee tuu senshureez laatur, abowt 9 pursent uv Residaz popyuulaashun livz in wun uv xe twentee
                                    sprawleeng undursee siteez."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa hiiyur levul."
                                    """;

    internal const string MenuThree = """
                                      "Alxoo setulmints hav bin establisht on Fristin, and on sevrul uv xe muunz uv xe gas jiiunt Blustin, xe vast majooritee uv of-woorldurz liv in xe spaas
                                      kolooneez establisht at Residaz troojin points."

                                      "Foor moor deetaald infoormaashun on xis tapik, konsult xe liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa hiiyur levul."
                                      """;

    internal override MenuItem Parent => new MainMenu();

    internal override List<MenuItem> Children =>
    [
        new() { Children = null, Parent = this, Text = MenuOne },
        new() { Children = null, Parent = this, Text = MenuTwo },
        new() { Children = null, Parent = this, Text = MenuThree }
    ];

    internal override string Text => MainMenu;
}