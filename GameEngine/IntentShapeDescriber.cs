using Model.Intent;

namespace GameEngine;

/// <summary>
///     Renders a parsed <see cref="IntentBase" /> as one compact line for the turn log (issue #578).
///     <para>
///     The engine already knew the intent type at <c>Debug</c> level, which production does not
///     retain — so diagnosing a parser-shape defect (issue #540: a preposition rewritten to a
///     hardcoded "with"; "press 3" arriving with no noun) meant inferring it from outcome
///     frequencies across a 1,352-command trace. Recording the shape makes those defects readable
///     straight off a single turn.
///     </para>
///     <para>
///     Deliberately hand-written rather than leaning on the records' generated <c>ToString</c>:
///     that would print <c>Message</c> and, for a global command, the whole command object, and it
///     would drift silently whenever a property is added. This prints only the fields that decide
///     dispatch — parse metadata, never raw player text, which <c>TurnLog.Input</c> already holds.
///     </para>
/// </summary>
internal static class IntentShapeDescriber
{
    internal static string? Describe(IntentBase? intent)
    {
        if (intent is null)
            return null;

        var name = intent.GetType().Name;

        return intent switch
        {
            SimpleIntent simple =>
                $"{name} verb='{simple.Verb}' noun='{simple.Noun}' adjective='{simple.Adjective}' adverb='{simple.Adverb}'",

            MultiNounIntent multi =>
                $"{name} verb='{multi.Verb}' nounOne='{multi.NounOne}' nounTwo='{multi.NounTwo}' preposition='{multi.Preposition}'",

            MoveIntent move => $"{name} direction='{move.Direction}' noun='{move.Noun}'",

            GlobalCommandIntent global => $"{name} command='{global.Command.GetType().Name}'",

            GoToDestinationIntent goTo => $"{name} destination='{goTo.Destination}'",

            EnterSubLocationIntent enter => $"{name} noun='{enter.Noun}'",

            ExitSubLocationIntent exit => $"{name} nounOne='{exit.NounOne}' nounTwo='{exit.NounTwo}'",

            TakeIntent take => $"{name} noun='{take.Noun}'",

            DropIntent drop => $"{name} noun='{drop.Noun}'",

            // NullIntent, InventoryIntent, LookIntent, PromptIntent, MultipleCommandsIntent: the
            // type name IS the whole shape.
            _ => name
        };
    }
}
