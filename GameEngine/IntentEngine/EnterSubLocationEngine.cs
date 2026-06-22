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
            // The noun resolved to a real, in-scope item that simply isn't a sub-location (issue
            // #262). If it's a fixed door/openable (the escape pod's BulkheadDoor, Zork's kitchen
            // window) that gates an "in" passage from here, "enter <door>" means "go through it":
            // defer to movement (Direction.In) so the location map applies its own open-check and
            // custom failure message ("The escape pod bulkhead is closed."), and once open actually
            // carries you through. This makes the bare noun "pod" behave like the full phrase
            // "escape pod" (already a Move), instead of falling through to a generic refusal the
            // narrator dressed up as a mock of an imaginary object.
            //
            // Two conditions gate the reroute:
            //   * The door must NOT be portable. A door is a fixture of the room; GetItemInScope also
            //     searches inventory, so without this an openable you are CARRYING (brown sack, egg,
            //     coffin) would resolve here and hijack the room's "in" exit — e.g. "enter sack" at
            //     Behind House would silently teleport you through the window. You walk through fixed
            //     doors, not things in your pack.
            //   * The location must actually have an "in" exit. A door gated on a different direction
            //     (an office door reached by going west) has no Direction.In entry, so Move(In) would
            //     land back on the generic refusal; say "You can't enter that." plainly instead.
            if (subLocation is IOpenAndClose && subLocation is not ICanBeTakenAndDropped
                && context.CurrentLocation.Navigate(Direction.In, context) is not null)
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