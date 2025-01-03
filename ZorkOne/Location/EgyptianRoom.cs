﻿using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class EgyptianRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<Temple>() } }
        };

    public override string Name => "Egyptian Room";

    protected override string GetContextBasedDescription(IContext context) =>
        "This is a room which looks like an Egyptian tomb. There is an ascending staircase to the west. ";

    public override void Init()
    {
        StartWithItem<Coffin>();
    }
}