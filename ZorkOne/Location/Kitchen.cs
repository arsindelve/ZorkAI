using GameEngine;
using GameEngine.IntentEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class Kitchen : LocationBase
{
    public override string Name => "Kitchen";

    public override string[] NounsForMatching => ["galley"];

    protected override string GetContextBasedDescription(IContext context)
    {
        return $"You are in the kitchen of the white house. A table seems to have been " +
               $"used recently for the preparation of food. A passage leads to the west " +
               $"and a dark staircase can be seen leading upward. A dark chimney leads down " +
               $"and to the east is a small window which is {(GetItem<KitchenWindow>().IsOpen ? "open" : "closed")}. ";
    }

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        if (input != null)
        {
            // Issue #344: symmetric to BehindHouse - object-based phrasing for going back out through
            // the window ("exit window", "board window", "through window", "go through window") means
            // the same thing as bare "out" / "east" here. See BehindHouse.RespondToSpecificLocationInteraction
            // for why this is handled on the raw input rather than relying on AI intent classification.
            var normalized = input.ToLowerInvariant().Trim().Replace("the ", "");
            if (normalized is "exit window" or "board window" or "through window" or "go through window")
            {
                var (resultObject, resultMessage) =
                    await new MoveEngine().Process(new MoveIntent { Direction = Direction.Out }, context, client);
                return resultObject ?? new PositiveInteractionResult(resultMessage);
            }
        }

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["table", "kitchen table"],
            "An ordinary wooden table, recently used — by the look of it — to prepare a meal. ",
            "The table is far too large and cumbersome to carry off. ")
    ];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var exit = new MovementParameters
        {
            // "exit window" resolves the window to this exit (DoorReroute). (issue #262)
            GatingItem = GetItem<KitchenWindow>(),
            CanGo = _ => GetItem<KitchenWindow>().IsOpen,
            CustomFailureMessage = "The kitchen window is closed.",
            Location = Repository.GetLocation<BehindHouse>()
        };

        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.Up, new MovementParameters { Location = GetLocation<Attic>() }
            },
            {
                Direction.W, new MovementParameters { Location = GetLocation<LivingRoom>() }
            },
            {
                Direction.E, exit
            },
            {
                Direction.Out, exit
            },
            {
                Direction.Down, new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "Only Santa Claus climbs down chimneys."
                }
            }
        };
    }

    public override void Init()
    {
        StartWithItem<KitchenWindow>();
        StartWithItem<BrownSack>();
        StartWithItem<Bottle>();
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(10);
        base.OnFirstTimeEnterLocation(context);
    }
}