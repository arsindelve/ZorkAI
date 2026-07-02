using GameEngine;
using GameEngine.IntentEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Item;
using Model.Movement;

namespace ZorkOne.Location;

public class BehindHouse : LocationBase
{
    public override string Name => "Behind House";

    public override string[] NounsForMatching => ["backyard", "back yard"];

    protected override string GetContextBasedDescription(IContext context)
    {
        return $"You are behind the white house. A path leads into the forest to the east. In one corner " +
               $"of the house there is a small window which is {(Repository.GetItem<KitchenWindow>().IsOpen ? "open" : "slightly ajar")}. ";
    }

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        if (input != null)
        {
            // Issue #344: object-based phrasing for going through the window ("enter window",
            // "board window", "through window", "go through window") means the same thing as the bare
            // "in" / "west" commands here, so walk through it via the same gated movement edge instead
            // of a generic refusal or no-op. This runs on the RAW input, before AI intent
            // classification, since the AI parser does not reliably tag these short phrasings as a
            // "board"/movement intent (unlike, say, "go to the kitchen through the window").
            var normalized = input.ToLowerInvariant().Trim().Replace("the ", "");
            if (normalized is "enter window" or "board window" or "through window" or "go through window")
            {
                var (resultObject, resultMessage) =
                    await new MoveEngine().Process(new MoveIntent { Direction = Direction.In }, context, client);
                return resultObject ?? new PositiveInteractionResult(resultMessage);
            }

            // Handle any other "through window" command (e.g. "look/peer/climb/crawl/squeeze/walk/move
            // through window") - these are exploratory phrasing, not a request to actually go through.
            if (normalized.Contains("through") && normalized.Contains("window"))
            {
                var window = Repository.GetItem<KitchenWindow>();
                if (window.IsOpen)
                {
                    return new PositiveInteractionResult("The window is open. If you want to enter the house, just say so. ");
                }
                else
                {
                    return new PositiveInteractionResult("The window is slightly ajar, but not enough to permit entry. ");
                }
            }
        }

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        MovementParameters enterKitchen = new()
        {
            // "enter window" resolves the window to this exit (DoorReroute). (issue #262)
            GatingItem = Repository.GetItem<KitchenWindow>(),
            CanGo = _ => Repository.GetItem<KitchenWindow>().IsOpen,
            CustomFailureMessage = "The kitchen window is closed. ",
            Location = Repository.GetLocation<Kitchen>()
        };

        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.E, new MovementParameters { Location = GetLocation<ClearingBehindHouse>() }
            },
            {
                Direction.S, new MovementParameters { Location = GetLocation<SouthOfHouse>() }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<NorthOfHouse>() }
            },
            {
                Direction.W, enterKitchen
            },
            {
                Direction.In, enterKitchen
            }
        };
    }

    public override void Init()
    {
        StartWithItem<KitchenWindow>();
    }
}