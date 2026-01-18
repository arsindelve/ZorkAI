using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Location;

public abstract class FloydSpecialInteractionLocation : LocationBase, IFloydSpecialInteractionLocation
{
    public bool InteractionHasHappened { get; set; }

    public abstract string FloydPrompt { get; }

    public override async Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var floyd = GetItem<Floyd>();

        if (!floyd.IsHereAndIsOn(context))
            return string.Empty;

        InteractionHasHappened = true;
        return Environment.NewLine + Environment.NewLine +
               await floyd.GenerateCompanionSpeech(context, generationClient, FloydPrompt);
    }
}