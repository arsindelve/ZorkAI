namespace Planetfall.Item.Lawanda.Library.Computer;

internal class GamesMenu : MenuItem
{
    internal const string MainMenu = """
                                         0. Maan Menyuu
                                         1. Zoork
                                         2. Dedliin and Witnis
                                         3. Starkros and Suspendid
                                     """;

    internal const string MenuOne = """
                                    "Xe Zoork triloojee, an adventshur klasik, taaks plaas in aa deeliitful but daanjuris undurgrownd seteeng."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa hiiyur levul."
                                    """;

    internal const string MenuTwo = """
                                    "Dedliin iz xe furst graat misturee uv xe kumpyuutur aaj, and Witnis iz its wurxee suksesur."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa hiiyur levul."
                                    """;

    internal const string MenuThree = """
                                      "Starkros iz Infookamz miind-bendeeng siiens-fikshun adventshur. Suspendid iz aa kriioojenik siiens-fikshun niitmaar."

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