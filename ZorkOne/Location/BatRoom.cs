using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.CoalMineLocation;

namespace ZorkOne.Location;

internal class BatRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            {
                Direction.S, new MovementParameters { Location = GetLocation<SqueakyRoom>() }
            },
            {
                Direction.E, new MovementParameters { Location = GetLocation<ShaftRoom>() }
            }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "You are in a small room which has doors only to the east and south. ";

    public override string Name => "Bat Room";

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        if (!context.HasItem<Garlic>())
        {
            context.CurrentLocation = GetLocation<MineEntrance>();
            var batText = """
                          A large vampire bat, hanging from the ceiling, swoops down at you!
                           
                              Fweep!
                              Fweep!
                              Fweep!
                           
                          The bat grabs you by the scruff of your neck and lifts you away....
                          """;

            return $"{batText}\n\n{context.CurrentLocation.GetDescription(context)} ";
        }

        return base.BeforeEnterLocation(context, previousLocation);
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        string response = "";
       
        if (context.HasItem<Garlic>())
            response +=
                "\nIn the corner of the room on the ceiling is a large vampire bat who is obviously deranged and holding his nose. ";

        response += LocationHelper.CheckSwordGlowingBrightly<Bat, BatRoom>(context);
        
        if (!string.IsNullOrEmpty(response))
            return Task.FromResult(response);

        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override void Init()
    {
        StartWithItem<Bat>();
        StartWithItem<JadeFigurine>();
    }
}