using GameEngine.Location;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

internal class ConferenceRoom : LocationBase
{
    public override string Name => "Conference Room";

    public override string[] NounsForMatching => ["meeting room", "boardroom"];

    private ConferenceRoomDoor Door => Repository.GetItem<ConferenceRoomDoor>();

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // The conference room door gates the passage south to the Rec Area. Declaring it as the
        // GatingItem lets "enter/exit door" resolve to this exit (DoorReroute). (issue #262)
        var doorPassage = new MovementParameters
        {
            GatingItem = Door,
            CanGo = _ => Door.IsOpen,
            Location = GetLocation<RecArea>(),
            CustomFailureMessage = "The door is closed. "
        };

        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<BoothOne>() },
            { Direction.S, doorPassage }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a fairly large room, almost filled by a round conference table. To the south is a door which " +
            $"is {(Door.IsOpen ? "open" : "closed")}. To the north is a small room about the size of a phone booth. ";
    }

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["conference table", "round conference table", "table"],
            "It's a large, round conference table. ",
            "The conference table is far too large to take. ")
    ];

    public override void Init()
    {
        StartWithItem<ConferenceRoomDoor>();
    }
}