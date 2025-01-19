using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Mech;

namespace Planetfall.Location;

public abstract class FloydSpecialInteractionLocation : LocationBase, IFloydSpecialInteractionLocation
{
    public bool InteractionHasHappened { get; set; }

    public abstract string FloydPrompt { get; }

    public override async Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        InteractionHasHappened = true;
        var floyd = GetItem<Floyd>();
        return Environment.NewLine + Environment.NewLine +
               await floyd.GenerateCompanionSpeech(context, generationClient, FloydPrompt);
    }
}