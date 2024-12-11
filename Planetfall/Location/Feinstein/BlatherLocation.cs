using GameEngine.Location;
using GameEngine.StaticCommand.Implementation;
using Model;
using Model.AIGeneration;
using Model.Location;
using Model.Movement;
using Planetfall.Item.Feinstein;

namespace Planetfall.Location.Feinstein;

internal abstract class BlatherLocation : LocationBase
{
    public override async Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        switch (VisitCount)
        {
            case 1:
                ItemPlacedHere<Blather>();
                return
                    "\n\nEnsign Blather, his uniform immaculate, enters and notices you are away from your post. " +
                    "\"Twenty demerits, Ensign Seventh Class!\" bellows Blather. \"Forty if you're not back on Deck " +
                    "Nine in five seconds!\" He curls his face into a hideous mask of disgust at your unbelievable negligence. ";
            case 3:
                {
                    context.CurrentLocation = Repository.GetLocation<Brig>();
                    return
                        "\n\nEnsign First Class Blather is standing before you, furiously scribbling demerits onto " +
                        "an oversized clipboard. Blather loses his last vestige of patience and drags you to the " +
                        "Feinstein's brig. He throws you in, and the door clangs shut behind you. \n\n" +
                        await new LookProcessor().Process(string.Empty, context, generationClient, Runtime.Unknown);
                }
            default:

                return "\n\nEnsign First Class Blather is standing before you, furiously scribbling demerits onto " +
                       "an oversized clipboard. \"I said to return to your post, Ensign Seventh Class!\" bellows " +
                       "Blather, turning a deepening shade of crimson. ";
        }
    }

    public override void Init()
    {

    }

    protected MovementParameters BlatherBlocksYou()
    {
        return new MovementParameters
        {
            CanGo = _ => false,
            CustomFailureMessage = RandomBlatherBlock()
        };
    }

    private string? RandomBlatherBlock()
    {
        string[] array =
        {
            "Blather blocks your path, growling about extra galley duty.",
            "Blather throws you to the deck and makes you do 20 push-ups.",
            "Ensign Blather pushes you roughly back toward your post.",
            "Ensign Blather blocks your way, snarling angrily."
        };
        var random = new Random();
        var index = random.Next(array.Length);
        return array[index];
    }
}