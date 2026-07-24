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
            // "enter the house" / "go into the house" name the structure the player is behind, and the only
            // way in from here is the (gated) window passage — the same movement "in"/"west" performs. The
            // AI parser classifies these inconsistently (often as a "goto" whose destination "house" then
            // matches the neighbouring "North of House"/"South of House" rooms and produces a nonsense
            // "which one?"), so resolve them deterministically on the raw input, exactly as the window
            // phrasings above. ("go in house" is already handled as a bare "in" by the direction parser.)
            if (normalized is "enter window" or "board window" or "through window" or "go through window"
                or "enter house" or "enter building" or "go into house" or "go inside house"
                or "walk into house" or "walk in house")
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