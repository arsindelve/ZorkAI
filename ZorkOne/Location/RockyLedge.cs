﻿using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class RockyLedge : LocationWithNoStartingItems
{
    public override string Name => "Rocky Ledge";

    protected override string GetContextBasedDescription(IContext context) =>
        "You are on a ledge about halfway up the wall of the river canyon. You can see from here that " +
        "the main flow from Aragain Falls twists along a passage which it is impossible for you to enter. " +
        "Below you is the canyon bottom. Above you is more cliff, which appears climbable.";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            {
                Direction.Up, new MovementParameters { Location = GetLocation<CanyonView>() }
            },
            {
                Direction.Down, new MovementParameters { Location = GetLocation<CanyonBottom>() }
            }
        };

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(
        string? input,
        IContext context, 
        IGenerationClient client
        )
    {
        switch (input?.ToLowerInvariant().Trim())
        {
            case "climb down":
                context.CurrentLocation = Repository.GetLocation<CanyonBottom>();
                string message = Repository.GetLocation<CanyonBottom>().GetDescription(context);
                return new PositiveInteractionResult(message);
        }

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }
}