namespace ZorkOne.Location;

public class MaintenanceRoom : DarkLocation{
    
     public override string Name => "Maintenance Room";

    protected override string ContextBasedDescription =>"This is what appears to have been the maintenance room for Flood Control Dam #3. Apparently, this room has been ransacked recently, for most of the valuable equipment is gone. On the wall in front of you is a group of buttons colored blue, yellow, brown, and red. There are doorways to the west and south.";

   protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<DamLobby>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<DamLobby>() } },
         };
    public override void Init()
    {
       
    }
}