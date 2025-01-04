namespace Planetfall.Item.Lawanda.Library.Computer;

internal class TechnologyMenu : MenuItem
{
    internal const string MainMenu = """
                                         0. Maan Menyuu
                                         1. Medisin
                                         2. Agrikultcur
                                         3. Tranzportaashun
                                         4. Roobotiks
                                         5. Planateree Sistumz
                                     """;

    internal const string MenuOne = """
                                    "Awl maajur dizeezuz hav bin kyuuribul foor oovur aa senshuree. Xe
                                    deevelupmint uv kriioojeniks now alowz dokturz tuu put paashints in
                                    staasis until aa kyuur iz fownd. Avurij Residan liif ekspektinsee
                                    iz now 147 revooluushunz."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe
                                    liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa
                                    hiiyur levul."
                                    """;

    internal const string MenuTwo = """
                                    "Durt farmeeng iz awl but obsooleet, wix moost fuud kumeeng frum xe
                                    hiidrooponiks kompleksiz oor xe undurwatur aljee farmz."

                                    "Foor moor deetaald infoormaashun on xis tapik, konsult xe
                                    liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa
                                    hiiyur levul."
                                    """;

    internal const string MenuThree = """
                                      "Planateree travul iz noormulee priivit skuutur foor shoort hops,
                                      and aarbus foor longur trips. Spaas travul haz reesintlee bin
                                      revooluushuniizd bii xe invenshun uv nuukleeur-fyuuld enjinz."

                                      "Foor moor deetaald infoormaashun on xis tapik, konsult xe
                                      liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa
                                      hiiyur levul."
                                      """;

    internal const string MenuFour = """
                                     "Untoold senshureez agoo, entiir teemz uv roobots wur reekwiird tuu
                                     purfoorm eevin xe simplist tasks...wun roobot wud handul viszuuwul
                                     funkshunz, wun roobot wud handul awditooree funkshunz, and soo
                                     foorx. Now, xanks tuu advansis in mineeatshurizaashun, xeez tasks
                                     kan bee purfoormd bii singul roobots, suc az xe multiipurpis B-19
                                     seereez."

                                     "Foor moor deetaald infoormaashun on xis tapik, konsult xe
                                     liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa
                                     hiiyur levul."
                                     """;

    internal const string MenuFive = """
                                     "Xe priimeree Planateree Sistumz ar Kors Kuntrool (foor
                                     maantaaneeng an iideel kliimit), Deefens (foor destroieeng
                                     pootenshulee daanjuris meeteeoorz), and xe reesintlee adid Prajekt
                                     Kuntrool (foor monitureeng proogres uv Xe Prajekt)."

                                     "Foor moor deetaald infoormaashun on xis tapik, konsult xe
                                     liibrereein foor xe aproopreeit spuulz. Tiip zeeroo tuu goo tuu aa
                                     hiiyur levul."
                                     """;

    internal override MenuItem? Parent => new MainMenu();

    internal override List<MenuItem> Children =>
    [
        new() { Children = null, Parent = this, Text = MenuOne },
        new() { Children = null, Parent = this, Text = MenuTwo },
        new() { Children = null, Parent = this, Text = MenuThree },
        new() { Children = null, Parent = this, Text = MenuFour },
        new() { Children = null, Parent = this, Text = MenuFive }
    ];

    internal override string Text => MainMenu;
}