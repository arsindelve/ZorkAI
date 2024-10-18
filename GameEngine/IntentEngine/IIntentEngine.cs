using Model.AIGeneration;
using Model.Interface;

namespace GameEngine.IntentEngine;

internal interface IIntentEngine
{
    Task<(InteractionResult? resultObject, string ResultMessage)> Process(IntentBase intent, IContext context, IGenerationClient generationClient);
}