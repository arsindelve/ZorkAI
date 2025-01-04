namespace Planetfall.Item.Lawanda.Library.Computer;

internal class HistoryMenu : MenuItem

{
    internal const string MainMenu = """
                                         0. Maan Menyuu
                                         1. Raashul Orijinz
                                         2. Graat Hiiaatus
                                         3. Riiz uv xe Nuu Teknakrasee
                                     """;

    internal const string MenuOne = """
                                    "Xe aancint lejindz saa xat ships frum xe Sekund Yuunyun wuns fild ar skiis and wil wun daa kum agen. Madern siientists, huu wuns dismist suc lejindz and
                                    felt xat liif eevolvd heer on Resida, now feel xat ar planit wuz reelee setuld bii men uv xe Sekund Yuunyun."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa hiiyur levul."
                                    """;

    internal const string MenuTwo = """
                                    "Wexur oor not xe lejindz uv xe Sekund Yuunyun ar truu, arkeeoloojists ar surtin xat aa peereeid uv hii teknoolojikul and sooshul deevelupmint egzistid
                                    xowzindz uv yeerz agoo, but foor sum reezin sivilizaashun slid intuu aa dark aaj lasteeng senshureez."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa hiiyur levul."
                                    """;

    internal const string MenuThree = """
                                      "Wixin xe last fiiv senshureez, xe riiz uv xe Nuu Teknakrasee haz reeturnd sivilizaashun tuu xe levul ataand beefoor xe Hiiaatus. Sooshul histooreeunz xink
                                      xat wen xe Dizeez struk, ar raas had aceevd aa levul uv sufistikaashun eekwal tuu xe pree-Hiiaatus."

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