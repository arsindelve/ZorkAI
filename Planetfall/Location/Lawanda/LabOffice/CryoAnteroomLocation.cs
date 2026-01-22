using GameEngine.Location;

namespace Planetfall.Location.Lawanda.LabOffice;

internal class CryoAnteroomLocation : LocationBase
{
    public override string Name => "Cryogenic Anteroom";

    public override void Init()
    {
        // No items in this location
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You have reached the cryogenic anteroom deep below the station. " +
            "The air is cold and sterile. Rows of cryo-sleep chambers line the walls, " +
            "most of them still functioning. You are safe here from the mutants above. " +
            "There is a sense of accomplishment - you've survived the escape! ";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // No exits - this is the end of the game sequence
        return new Dictionary<Direction, MovementParameters>();
    }
}
