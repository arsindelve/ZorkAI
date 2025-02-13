using Amazon.DynamoDBv2.Model;

namespace Planetfall.Item.Lawanda.Lab;

internal class RadiationLockInnerDoor : SimpleDoor
{
    public override string[] NounsForMatching =>
    [
        "radiation-lock door", "door", "radiation lock", "radiation", "radiation lock door", "radiation-lock", "radiation door"
    ];

    public override string CannotBeOpenedDescription(IContext context)
    {
        if (Repository.GetItem<RadiationLockOuterDoor>().IsOpen)
            return "A very bored-sounding recorded voice explains that, in order to prevent contamination, both lock " +
                   "doors cannot be open simultaneously. ";
        
        return base.CannotBeOpenedDescription(context);
    }
}