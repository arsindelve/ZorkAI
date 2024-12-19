using Amazon.LexRuntimeV2.Model;
using Microsoft.Extensions.Logging;
using Model;
using Model.AIParsing;
using Model.Intent;
using Model.Movement;

namespace Lex;

public class LexParser(ILogger? logger) : IAIParser
{
    private readonly LexClient _client = new();

    public async Task<IntentBase> AskTheAIParser(string input, string locationDescription, string sessionId)
    {
        logger?.LogDebug($"Text: '{input}' was sent to Amazon Lex");
        var parserResponse = await _client.SendTextMsgToLex(input, sessionId);

        var intentName = ExtractIntentName(parserResponse);
        var score = ExtractIntentConfidence(parserResponse);

        if (parserResponse.Messages.Any() && score.GetValueOrDefault() > 0.85)
            return BuildPromptIntent(parserResponse);

        switch (intentName)
        {
            case "Go":
                if (score.GetValueOrDefault() < 0.85)
                    break;

                return BuildMovementIntent(parserResponse);

            case "MultiNoun":

                if (score.GetValueOrDefault() < 0.50)
                    break;

                return BuildMultiNounIntent(parserResponse, input);

            default:

                if (score.GetValueOrDefault() < 0.90)
                    return new NullIntent();

                return BuildSimpleInteractionIntent(parserResponse, input, intentName);
        }

        return new NullIntent();
    }

    public string LanguageModel => "Lex";

    private IntentBase BuildMultiNounIntent(RecognizeTextResponse lexTextResponse, string input)
    {
        var itemOne = lexTextResponse.Interpretations?.FirstOrDefault()?.Intent?.Slots["Item1"]?.Value
            ?.InterpretedValue;

        var itemTwo = lexTextResponse.Interpretations?.FirstOrDefault()?.Intent?.Slots["Item2"]?.Value
            ?.InterpretedValue;

        var verb = lexTextResponse.Interpretations?.FirstOrDefault()?.Intent?.Slots["Verb"]?.Value
            ?.InterpretedValue;

        var preposition = lexTextResponse.Interpretations?.FirstOrDefault()?.Intent?.Slots["Preposition"]?.Value
            ?.InterpretedValue;

        if (string.IsNullOrEmpty(verb) || string.IsNullOrEmpty(itemOne) ||
            string.IsNullOrEmpty(preposition) || string.IsNullOrEmpty(itemTwo))
            return new NullIntent();

        return new MultiNounIntent
        {
            Verb = verb,
            NounOne = itemOne,
            NounTwo = itemTwo,
            Preposition = preposition,
            OriginalInput = input
        };
    }

    private static double? ExtractIntentConfidence(RecognizeTextResponse parserResponse)
    {
        return parserResponse.Interpretations?.FirstOrDefault()
            ?.NluConfidence?.Score;
    }

    private static string ExtractIntentName(RecognizeTextResponse parserResponse)
    {
        return parserResponse.Interpretations.First().Intent.Name;
    }

    private static IntentBase BuildPromptIntent(RecognizeTextResponse lexTextResponse)
    {
        return new PromptIntent { Message = lexTextResponse.Messages.First().Content };
    }

    private static IntentBase BuildSimpleInteractionIntent(RecognizeTextResponse lexTextResponse, string rawInput,
        string inputType)
    {
        string? adverb = null;

        var item = lexTextResponse.Interpretations?.FirstOrDefault()?.Intent?.Slots["Item"]?.Value
            ?.InterpretedValue;

        var verb = lexTextResponse.Interpretations?.FirstOrDefault()?.Intent?.Slots["Verb"]?.Value
            ?.InterpretedValue;

        if (inputType == "SimpleActionWithAdverb")
            adverb = lexTextResponse.Interpretations?.FirstOrDefault()?.Intent?.Slots["Adverb"]?.Value
                ?.InterpretedValue;

        if (string.IsNullOrEmpty(verb) || string.IsNullOrEmpty(item))
            return new NullIntent();

        return new SimpleIntent
        {
            OriginalInput = rawInput,
            Verb = verb,
            Noun = item,
            Adverb = adverb
        };
    }

    private static IntentBase BuildMovementIntent(RecognizeTextResponse lexTextResponse)
    {
        var direction = lexTextResponse.Interpretations
            ?.FirstOrDefault()?.Intent
            ?.Slots["Direction"].Value
            .InterpretedValue;

        return new MoveIntent
        {
            Direction = DirectionParser.ParseDirection(direction)
        };
    }
}