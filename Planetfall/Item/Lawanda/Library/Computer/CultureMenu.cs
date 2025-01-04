namespace Planetfall.Item.Lawanda.Library.Computer;

internal class CultureMenu : MenuItem
{
    internal const string MainMenu = """
                                        0. Maan Menyuu
                                        1. Lituracur
                                        2. Art
                                        3. Muusik
                                    """;

    internal const string MenuOne = """
                                    "Menee volyuumz on xe deevelupmint uv Residan lituracur ar on fiil in xe liibreree. Alsoo, kopeez uv awl graat wurks uv riiteeng, sum daateeng bak tuu xe
                                    mixikul daaz uv xe Sekund Yuunyun, ar lookaatid heer."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa hiiyur levul."
                                    """;

    internal const string MenuTwo = """
                                    "Histoorikul studeez and reeproodukshunz uv Residan art ar avaalibul heer foor awl xree maajur peereeids uv art deevelupmint: xe Primitiv peereeid, xe
                                    Renasans peereeid uv xe urlee poost-Hiiaatus, and xe moost reesint peereeid uv videeoo and laazur art."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa hiiyur levul."
                                    """;

    internal const string MenuThree = """
                                      "Reekoordeengz uv awl impoortint kompoozishunz uv xe last fiiv hundrid yeerz ar lookaatid in xe liibrereez data banks."

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