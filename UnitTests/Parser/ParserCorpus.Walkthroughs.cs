using GameEngine;
using Model.Intent;
using Model.Movement;
using UnitTests.Parser.Walkthroughs;

namespace UnitTests.Parser;

/// <summary>
///     The cases nobody wrote: every sentence the walkthroughs type that changes the game, with every
///     reading of it the game answers identically (see <see cref="WalkthroughHarvest" />). They are the
///     commands that need setup or two nouns - the walkthrough is the setup - and they are checked at
///     every layer exactly as the written cases are, by a scripted AI or a live one.
/// </summary>
public static partial class ParserCorpus
{
    private static IEnumerable<ParserCase> FromWalkthroughs(string game, string fileName,
        IEnumerable<ParserCase> written)
    {
        // A sentence somebody wrote a case for keeps that case: it says more about what is meant.
        var taken = written.Select(@case => @case.Input).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var interactions = WalkthroughHarvest.Load(fileName);

        if (interactions.Count == 0)
            yield break;

        var grammar = new DeterministicParser(game);

        foreach (var interaction in interactions)
        {
            if (!taken.Add(interaction.Input))
                continue;

            var expected = Expected(interaction.Readings);

            // Which layer answers is asked of the grammar, not recorded: these cases say what a sentence
            // means, and the written ones are what pin where the grammar's reach ends.
            var byGrammar = grammar.Parse(interaction.Input) is not null;

            yield return new ParserCase
            {
                Game = game,
                Location = interaction.Room,
                Input = interaction.Input,
                ResolvedBy = byGrammar ? ResolvedBy.Grammar : ResolvedBy.Ai,
                ModelAnswer = Answer(interaction.Readings[0]),
                FromAi = expected,
                FromGrammar = byGrammar ? expected : null
            };
        }
    }

    /// <summary>The readings as a chain of alternatives: satisfying any one of them is enough.</summary>
    private static IntentExpectation Expected(IReadOnlyList<Reading> readings)
    {
        IntentExpectation? next = null;

        foreach (var reading in readings.Reverse())
            next = Expected(reading, next);

        return next!;
    }

    private static IntentExpectation Expected(Reading reading, IntentExpectation? alternative)
    {
        return reading.Kind switch
        {
            "act" when reading.SecondNouns is { Length: > 0 } => new IntentExpectation
            {
                IntentType = typeof(MultiNounIntent), Verbs = reading.Verbs, NounOne = reading.Nouns,
                NounTwo = reading.SecondNouns, Prepositions = [reading.Preposition!], Alternative = alternative
            },
            "act" => new IntentExpectation
            {
                IntentType = typeof(SimpleIntent), Verbs = reading.Verbs, NounOne = reading.Nouns,
                Alternative = alternative
            },
            "take" => new IntentExpectation
                { IntentType = typeof(TakeIntent), NounOne = reading.Nouns, Alternative = alternative },
            "drop" => new IntentExpectation
                { IntentType = typeof(DropIntent), NounOne = reading.Nouns, Alternative = alternative },
            "board" => new IntentExpectation
                { IntentType = typeof(EnterSubLocationIntent), NounOne = reading.Nouns, Alternative = alternative },
            "disembark" => new IntentExpectation
                { IntentType = typeof(ExitSubLocationIntent), NounOne = reading.Nouns, Alternative = alternative },
            "goto" => new IntentExpectation
                { IntentType = typeof(GoToDestinationIntent), NounOne = reading.Nouns, Alternative = alternative },
            "move" => new IntentExpectation
            {
                IntentType = typeof(MoveIntent), Direction = DirectionParser.ParseDirection(reading.Direction),
                NounOne = reading.Nouns.Length == 0 ? null : reading.Nouns, Alternative = alternative
            },
            _ => throw new InvalidOperationException($"Unknown kind of reading '{reading.Kind}'.")
        };
    }

    /// <summary>What a model that read the sentence as the walkthrough does would answer.</summary>
    private static ModelAnswer Answer(Reading reading)
    {
        string[] nouns = reading.SecondNouns is { Length: > 0 }
            ? [reading.Nouns[0], reading.SecondNouns[0]]
            : reading.Nouns.Take(1).ToArray();

        return new ModelAnswer(reading.Kind, reading.Verbs[0], nouns, reading.Preposition,
            reading.Direction is null ? null : SchemaWord(DirectionParser.ParseDirection(reading.Direction)));
    }

    /// <summary>The word the parser's schema has for a direction.</summary>
    private static string SchemaWord(Direction direction)
    {
        return direction switch
        {
            Direction.N => "north",
            Direction.S => "south",
            Direction.E => "east",
            Direction.W => "west",
            Direction.NE => "north-east",
            Direction.NW => "north-west",
            Direction.SE => "south-east",
            Direction.SW => "south-west",
            _ => direction.ToString().ToLowerInvariant()
        };
    }
}
