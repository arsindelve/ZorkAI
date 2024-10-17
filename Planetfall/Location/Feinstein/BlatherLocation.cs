using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Location;

namespace Planetfall.Location.Feinstein;

internal abstract class BlatherLocation : LocationWithNoStartingItems
{
    public override string AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient? generationClient)
    {
        return VisitCount switch
        {
            1 =>
                "\nEnsign Blather, his uniform immaculate, enters and notices you are away from your post. \"Twenty demerits, Ensign Seventh Class!\" bellows Blather. \"Forty if you're not back on Deck Nine in five seconds!\" He curls his face into a hideous mask of disgust at your unbelievable negligence. ",
            3 =>
                "\nEnsign First Class Blather is standing before you, furiously scribbling demerits onto an oversized clipboard. Blather loses his last vestige of patience and drags you to the Feinstein's brig. He throws you in, and the door clangs shut behind you. ",
            _ =>
                "\nEnsign First Class Blather is standing before you, furiously scribbling demerits onto an oversized clipboard. \"I said to return to your post, Ensign Seventh Class!\" bellows Blather, turning a deepening shade of crimson. "
        };
    }
}