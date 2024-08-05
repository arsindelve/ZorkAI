﻿using GameEngine.Location;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Location;

// This is not a dark location either, oddly. 
public class Altar : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<Temple>() } },
            { Direction.Down, new MovementParameters
            {
                WeightLimit = 15, 
                WeightLimitFailureMessage = "You haven't a prayer of getting down there with what you're carrying. ", 
                Location = GetLocation<CaveSouth>()
            } }
        };

    public override string Name => "Altar";

    protected override string ContextBasedDescription =>
        "This is the south end of a large temple. In front of you is what appears to be an altar. " +
        "In one corner is a small hole in the floor which leads into darkness. You probably could not get back up it. ";

    public override InteractionResult RespondToSpecificLocationInteraction(string? action, IContext context)
    {
        if (!action?.Trim().StartsWith("pray", StringComparison.InvariantCultureIgnoreCase) ?? false)
            return base.RespondToSpecificLocationInteraction(action, context);

        var newLocation = GetLocation<ForestOne>();
        context.CurrentLocation = newLocation;
        return new PositiveInteractionResult(newLocation.Description + Environment.NewLine);
    }

    public override void Init()
    {
        StartWithItem<BlackBook>();
        StartWithItem<Candles>();

        GetItem<Candles>().IsOn = true;
    }
}