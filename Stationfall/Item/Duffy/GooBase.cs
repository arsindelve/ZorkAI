namespace Stationfall.Item.Duffy;

/// <summary>
///     Shared behavior for the two blobs of Patrol ration goo in the survival kit. The goo is too gooey
///     to carry loose — it must be eaten straight from the kit (ship.zil:1395-1405).
/// </summary>
public abstract class GooBase : ItemBase, ICanBeExamined, ICanBeEaten
{
    /// <summary>
    ///     What the ration is optimistically labelled as.
    /// </summary>
    protected abstract string OfficialName { get; }

    protected abstract string Color { get; }

    public override int Size => 2;

    public string ExaminationDescription =>
        $"A blob of {Color} goo, officially {OfficialName}. It jiggles when the deck plates hum. ";

    public (string Message, bool WasConsumed) OnEating(IContext context)
    {
        if (context is StationfallContext stationfallContext)
        {
            if (!stationfallContext.IsHungry)
                return (StationfallContext.NotHungryMessage, false);

            stationfallContext.Eat();
        }

        return ($"You scoop up the {Color} goo and eat it. It tastes the way {OfficialName} might taste " +
                "if it had been described to the cook over a bad comm link. ", true);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return $"A blob of {Color} goo";
    }
}
