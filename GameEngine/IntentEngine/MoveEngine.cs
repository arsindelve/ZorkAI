using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace GameEngine.IntentEngine;

public class MoveEngine : IIntentEngine
{
    internal virtual IRandomChooser Chooser => new RandomChooser();
    
    public async Task<(InteractionResult? resultObject, string ResultMessage)> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        // TODO: Move from a dark location to another dark location and you die. 

        if (intent is not MoveIntent moveTo)
            throw new ArgumentException("Cast error");
        
        context.LastMovementDirection = moveTo.Direction;
        
        MovementParameters? movement = context.CurrentLocation.Navigate(moveTo.Direction, context);

        if (movement == null)
        {
            var entered = await TryEnterTheNamedObject(moveTo, context, generationClient);
            if (entered is not null)
                return entered.Value;

            return (null, await GetGeneratedCantGoThatWayResponse(generationClient, moveTo.Direction.ToString(), context));
        }
        
        if (movement.WeightLimit < context.CarryingWeight)
            return (null, movement.WeightLimitFailureMessage);

        if (!movement.CanGo(context) || movement.Location == null)
            return (null, !string.IsNullOrEmpty(movement.CustomFailureMessage)
                ? movement.CustomFailureMessage + Environment.NewLine
                : await GetGeneratedCantGoThatWayResponse(generationClient, moveTo.Direction.ToString(), context));

        // Forget pronoun antecedents for items we left behind in the previous room, but keep them
        // for items the player is still carrying — "it"/"them" should still resolve to a carried
        // item after walking to the next room (issue #248).
        if (!context.HasMatchingNoun(context.LastNoun).HasItem)
            context.LastNoun = "";
        context.LastNouns = context.LastNouns
            .Where(n => context.HasMatchingNoun(n).HasItem).ToList();

        return (null, await Go(context, generationClient, movement));
    }

    /// <summary>
    ///     Issue #551. "enter &lt;thing&gt;" is a board command, but the AI parser's direction list also
    ///     contains the word "enter", so gpt-4o routinely buckets bare "enter door" as a move in
    ///     <see cref="Direction.In" /> with the door merely tagged as a noun. In the Planetfall Elevator
    ///     Lobby - a room with two doors and no In exit - that meant a real player got "You cannot go
    ///     that way." where the #532 fix promises "Do you mean the lower or the upper elevator door?".
    ///     The deterministic TestParser maps "enter door" straight to an EnterSubLocationIntent, so the
    ///     suite never saw it.
    ///     <para>
    ///         Deliberately narrow, so this can only turn a refusal into the action the player asked for
    ///         and never redirect a move that already worked: it runs only after the map has said there
    ///         is NO exit this way at all, only for In (the direction the word "enter" produces), and
    ///         only when the noun they typed names something actually in scope. Everything else - the
    ///         "which door?" question, a sub-location, a door to walk through, the plain refusal for a
    ///         noun you can't enter - is <see cref="EnterSubLocationEngine" />'s existing job.
    ///     </para>
    /// </summary>
    private static async Task<(InteractionResult? resultObject, string ResultMessage)?> TryEnterTheNamedObject(
        MoveIntent moveTo, IContext context, IGenerationClient generationClient)
    {
        if (moveTo.Direction != Direction.In || string.IsNullOrWhiteSpace(moveTo.Noun))
            return null;

        if (Repository.GetItemInScope(moveTo.Noun, context) is null)
            return null;

        return await new EnterSubLocationEngine().Process(
            new EnterSubLocationIntent { Noun = moveTo.Noun, Message = moveTo.Message }, context, generationClient);
    }

    public static async Task<string> Go(IContext context, IGenerationClient generationClient, MovementParameters movement)
    {
        var previousLocation = context.CurrentLocation;
        context.CurrentLocation.OnLeaveLocation(context, movement.Location!, previousLocation);
        context.CurrentLocation = movement.Location!;

        var transitionMessage = movement.TransitionMessage ?? string.Empty;
        var beforeEnteringText = movement.Location!.BeforeEnterLocation(context, previousLocation);
        var processorText = LookProcessor.LookAround(context);
        var afterEnteringText = await movement.Location.AfterEnterLocation(context, previousLocation, generationClient);

        var result = transitionMessage + beforeEnteringText + processorText + afterEnteringText + Environment.NewLine;
        return result;
    }

    private async Task<string> GetGeneratedCantGoThatWayResponse(IGenerationClient generationClient, string direction,
        IContext context)
    {
        // If generation is disabled, always return standard response
        if (generationClient.IsDisabled)
            return "You cannot go that way. ";

        // 20% of the time, let's generate a response. Otherwise, give the standard response
        if (!Chooser.RollDiceSuccess(5))
            return "You cannot go that way. ";

        var request =
            new CannotGoThatWayRequest(context.CurrentLocation.GetDescriptionForGeneration(context), direction);
        var result = await generationClient.GenerateNarration(request, context.SystemPromptAddendum);
        return result;
    }
}