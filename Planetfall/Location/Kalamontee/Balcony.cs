﻿using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

public class Balcony : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.Down, ((PlanetfallContext)context).Day == 1 ? Go<Crag>() : Go<Underwater>() },
            { Direction.Up, Go<WindingStair>() }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This is an octagonal room, half carved into and half built out from the cliff wall. " +
        "Through the shattered windows which ring the outer wall you can see ocean to the horizon. " +
        "A weathered metal plaque with barely readable lettering rests below the windows. The language " +
        "seems to be a corrupt form of Galalingua. A steep stairway, roughly cut into the face of the " +
        "cliff, leads upward. " + DayChange(context as PlanetfallContext);

    private string DayChange(PlanetfallContext? context)
    {
        return context?.Day switch
        {
            1 => "A rocky crag can be seen about eight meters below. ",
            2 => "The ocean waters swirl below. The crag where you landed yesterday is now underwater! ",
            3 => "Ocean waters are lapping at the base of the balcony. ",
            _ => ""
        };
    }

    public override string Name => "Balcony";
    public override void Init()
    {
        StartWithItem<Plaque>();
    }
}