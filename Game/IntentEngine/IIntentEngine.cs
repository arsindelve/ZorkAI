using Model.AIGeneration;
using Model.Interface;

namespace Game.IntentEngine;

internal interface IIntentEngine
{
    Task<string> Process(IntentBase intent, IContext context, IGenerationClient generationClient);
}