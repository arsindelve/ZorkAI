using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Intent;
using Model.Interface;
using Model.Item;
using Model.Location;
using Model.Movement;

namespace GameEngine.IntentEngine;

internal class EnterSubLocationEngine : IIntentEngine
{
    public async Task<(InteractionResult? resultObject, string ResultMessage)> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not EnterSubLocationIntent enter)
            throw new ArgumentException("Cast error");

        if (string.IsNullOrEmpty(enter.Noun))
            throw new ArgumentException("Null or empty noun. What's up with that?");

        IItem? subLocation = Repository.GetItemInScope(enter.Noun, context);
        if (subLocation == null)
            return (null, await GetGeneratedCantGoThatWayResponse(generationClient, context, enter.Noun));

        if (subLocation is not ISubLocation subLocationInstance)
        {
            // The noun resolved to a real, in-scope item that simply isn't a sub-location. Two cases
            // (issue #262):
            //   1. It's a door/openable (e.g. the escape pod's BulkheadDoor, or Zork's kitchen
            //      window). "enter <door>" means "go through it", so defer to movement (Direction.In)
            //      and let the location map apply its own open-check and custom failure message
            //      ("The escape pod bulkhead is closed."). This makes the bare noun "pod" behave
            //      exactly like the full phrase "escape pod" (already a Move). Without it, a valid
            //      noun fell through to a generic refusal that the narrator dressed up as a mock of
            //      an imaginary object.
            //   2. Anything else (a machine, a sword): you genuinely can't enter it. Say so plainly
            //      rather than letting the narrator mock a perfectly valid object.
            if (subLocation is IOpenAndClose)
                return await new MoveEngine().Process(
                    new MoveIntent { Direction = Direction.In }, context, generationClient);

            return (null, "You can't enter that. ");
        }

        if (context.CurrentLocation.SubLocation == subLocationInstance)
            return (null, $"You're already in the {enter.Noun}. ");

        return (null, subLocationInstance.GetIn(context));
    }

    private static async Task<string> GetGeneratedCantGoThatWayResponse(IGenerationClient generationClient,
        IContext context, string noun)
    {
        // If generation is disabled, return standard response
        if (generationClient.IsDisabled)
            return "You cannot go that way. ";

        var request = new CannotEnterSubLocationRequest(context.CurrentLocation.GetDescriptionForGeneration(context), noun);
        var result = await generationClient.GenerateNarration(request, context.SystemPromptAddendum);
        return result;
    }
}