using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using ZorkOne.Command;

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

    public override string Name => "Dome Room";

    protected override string ContextBasedDescription =>
        "You are at the periphery of a large dome, which forms the ceiling of another room below. " +
        "Protecting you from a precipitous drop is a wooden railing which circles the dome. ";

    public override void Init()
    {
    }

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context, IGenerationClient client)
    {
        switch (input)
        {
            case "jump":
                var death =
                    "This was not a very safe place to try jumping.\nYou should have looked before you leaped. \n";
                return new DeathProcessor().Process(death, context);
        }

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }

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