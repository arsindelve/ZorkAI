namespace Stationfall.Location.Duffy;

/// <summary>
///     A cathedral-sized warehouse holding nothing but boxed forms (ship.zil:160-169). Pure red
///     herring — there is nothing here to take, and the only exit is back the way you came.
/// </summary>
public class FormStorageRoom : LocationBase
{
    public override string Name => "Forms Storage Room";

    public override string[] NounsForMatching => ["forms storage room", "storage room", "form storage"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<DeckTwelve>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is the largest space aboard the ship — three decks tall, with the floor area of " +
               "several buzzball fields, and filled to the ceiling with pallets of boxes. The only " +
               "exit is fore. ";
    }

    public override void Init()
    {
        StartWithItem<Pallets>();
        StartWithItem<BoxedForms>();
    }
}
