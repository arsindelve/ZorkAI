namespace Planetfall.Item.Lawanda.Library.Computer;

internal class ProjectMenu : MenuItem
{
    internal const string MainMenu = """
                                         0. Maan Menyuu
                                         1. Orijinz uv xe Dizeez
                                         2. Xe Instalaashunz
                                         3. Prajekt Kuntrool
                                     """;

    internal const string MenuOne = """
                                    "Xe oorijin uv Xe Dizeez haz bin linkt tuu xe Sentur foor Advanst Kriioojenik Reesurc, wic wuz kondukteeng reesurc intuu waaz uv ekstendeeng xe Kriioojenik
                                    peereeid indefinitlee. Alxoo xis reesurc wuz aa sukses, sumhow Xe Dizeez wuz reeleest and beegan spredeeng."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa hiiyur levul."
                                    Floyd produces a crayon from one of his compartments and scrawls his name on the wall.
                                    """;

    internal const string MenuTwo = """
                                    "Xe tuu kompleksiz wur establisht on xe twin peek platooz uv Kalamontee and Lawanda. Xeez lookaashunz wur coozin beekawz xaar hiit wud maak transpoortaashun
                                    and komyuunikaashunz eezeeur, and soo xat xe vast reeakturz and kriioojeniks caamburz kud bee kunstruktid in xe mowntinz beeloo."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa hiiyur levul."
                                    """;

    internal const string MenuThree = """
                                      "Faaz Wun: xe kunstrukshun uv xe Kalamontee and Lawanda Kompleksiz. Faaz Tuu: mass kriioojenik freezeeng uv Residan popyuulaashun. Faaz Xree: siimultaaneeus
                                      monitureeng uv kriioojeniks wiil awtoomaatid reesurc iz konduktid bii inkrediblee soofistikaatid kumpyuuturiizd fasiliteez. Faaz Foor: reeviivul and
                                      inokyuulaashun uv xe popyuulaashun."
                                        
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