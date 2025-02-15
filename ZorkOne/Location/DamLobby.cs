﻿using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class DamLobby : LocationBase
{
    public override string Name => "Dam Lobby";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, new MovementParameters { Location = GetLocation<Dam>() } },
            {
                Direction.N, new MovementParameters
                {
                    Location = GetLocation<MaintenanceRoom>(), CanGo = _ => !GetLocation<MaintenanceRoom>().RoomFlooded,
                    CustomFailureMessage = "The room is full of water and cannot be entered. "
                }
            },
            {
                Direction.E, new MovementParameters
                {
                    Location = GetLocation<MaintenanceRoom>(), CanGo = _ => !GetLocation<MaintenanceRoom>().RoomFlooded,
                    CustomFailureMessage = "The room is full of water and cannot be entered. "
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This room appears to have been the waiting room for groups touring the dam. There are open doorways here " +
            "to the north and east marked \"Private\", and there is a path leading south over the top of the dam.\n";
    }

    public override void Init()
    {
        StartWithItem<Guidebook>();
        StartWithItem<Matchbook>();
    }
}