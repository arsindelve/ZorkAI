using Stationfall.Command;

namespace Stationfall.Item.Duffy;

/// <summary>
///     Rex — a heavy-lifting robot in bin one, and a trap. Sixteen tons of Rex is not good at stopping
///     when you do; picking him gets the player killed the moment he follows them into the Cargo Bay
///     Entrance (ship.zil:843-852, 880-885).
/// </summary>
public class Rex : ShipRobot
{
    public override int BinNumber => 1;

    public override string[] NounsForMatching => ["rex", "heavy robot", "bulky robot"];

    public override string InTheBinDescription =>
        "The first bin holds a bulky robot built for heavy lifting, all pneumatic arm-lifts and " +
        "reinforced bracing. A brass plate reads \"Rex.\" ";

    public override string ExaminationDescription =>
        "Rex is an enormous lifting robot. He regards you with the blank, patient incomprehension of " +
        "sixteen tons of machinery that has never once had an idea. ";

    protected override string GreetingResponse => "\"Yeah. Hey.\" ";

    protected override string FollowResponse =>
        IsSelected
            ? "\"I go where they puts me, an' they put me with youse.\" "
            : "\"Can't. Ain't been assigned ta youse.\" ";

    protected override string CatchAllResponse => "Rex looks at you without the least sign of comprehension. ";

    protected override string FollowThePlayer(IContext context)
    {
        MoveToPlayer(context);

        // The kill is resolved here, as Rex arrives, rather than from the room's own turn. Two actors
        // both reacting to the same arrival would depend on the order they happen to run in, which
        // would make the death fire a turn late (or not at all).
        if (context.CurrentLocation is CargoBayEntrance)
        {
            context.RemoveActor(this);

            return new DeathProcessor().Process(
                "Rex trundles in after you. Unfortunately, Rex is not especially bright on his best " +
                "days, and this is not one of them: he neglects to stop when you do. Sixteen tons of " +
                "lifting robot turn you into a very thin layer of Lieutenant. ",
                context).InteractionMessage;
        }

        return "Rex trundles after you, the deck plates flexing under his weight. ";
    }
}
