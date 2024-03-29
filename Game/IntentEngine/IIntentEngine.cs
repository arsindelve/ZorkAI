using Model.AIGeneration;

namespace Game.IntentEngine;

internal interface IIntentEngine
{
    Task<string> Process(IntentBase intent, IContext context, IGenerationClient generationClient);
}