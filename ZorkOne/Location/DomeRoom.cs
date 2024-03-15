﻿using Model.Intent;

namespace ZorkOne.Location;

public class DomeRoom : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<EngravingsCave>() } },
            {
                Direction.Down,
                new MovementParameters
                {
                    CanGo = _ => GetItem<Rope>().TiedToRailing,
                    Location = GetLocation<TorchRoom>(),
                    CustomFailureMessage = "You cannot go down without breaking many bones."
                }
            }
        };

    protected override string Name => "Dome Room";

    // TODO: Implement fatal jump. 

    protected override string ContextBasedDescription =>
        "You are at the periphery of a large dome, which forms the ceiling of another room below. " +
        "Protecting you from a precipitous drop is a wooden railing which circles the dome. ";

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        string[] verbs = ["tie", "attach"];
        string[] prepositions = ["to", "onto", "on"];

        if (!action.NounOne.ToLowerInvariant().Trim().Contains("rope"))
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.NounTwo.ToLowerInvariant().Trim().Contains("railing"))
            return base.RespondToMultiNounInteraction(action, context);

        if (!verbs.Contains(action.Verb.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        if (!prepositions.Contains(action.Preposition.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        if (!context.HasItem<Rope>() && HasItem<Rope>())
            return new PositiveInteractionResult("You don't have the rope.");

        if (!context.HasItem<Rope>())
            return base.RespondToMultiNounInteraction(action, context);
        
        GetItem<Rope>().TiedToRailing = true;
        context.Drop(GetItem<Rope>());
        
        return new PositiveInteractionResult("The rope drops over the side and comes within ten feet of the floor. ");
    }
}

public class TorchRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<EngravingsCave>() } },
            {
                Direction.Up,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "You cannot reach the rope."
                }
            }
        };


    protected override string Name => "Torch Room";

    protected override string ContextBasedDescription =>
        "This is a large room with a prominent doorway leading to a down staircase. To the west is a narrow twisting tunnel, " +
        "covered with a thin layer of dust.  Above you is a large dome painted with scenes depicting elfin hacking rites. " +
        "Up around the edge of the dome (20 feet up) is a wooden railing. In the center of the room there is a white marble pedestal.";
}