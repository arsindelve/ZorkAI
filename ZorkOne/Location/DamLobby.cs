namespace ZorkOne.Location;

public class DamLobby : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<Dam>() } }
        };

    protected override string ContextBasedDescription =>
        "This room appears to have been the waiting room for groups touring the dam. There are open doorways here " +
        "to the north and east marked \"Private\", and there is a path leading south over the top of the dam.";

    public override string Name => "Dam Lobby";

    public override void Init()
    {
        StartWithItem(GetItem<Matchbook>(), this);
    }
}