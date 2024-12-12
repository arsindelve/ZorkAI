using Planetfall.Command;

namespace Planetfall.Location.Feinstein;

internal static class ExplosionCoordinator
{
    private static string YouExploded(IContext context, string result)
    {
        context.RemoveActor(Repository.GetLocation<DeckNine>());
        return new DeathProcessor().Process(result, context).InteractionMessage;
    }

    internal static string HandleExplosion(IContext context)
    {
        var action = "";

        switch (context.Moves)
        {
            case DeckNine.TurnWhenFeinsteinBlowsUp:

                // Start the timer on the escape pod leaving. 
                context.RegisterActor(Repository.GetLocation<EscapePod>());

                // Open the bulkhead door to the escape pod. 
                Repository.GetItem<BulkheadDoor>().IsOpen = true;
                action +=
                    $"\n\nA massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. " +
                    $"{(context.CurrentLocation is DeckNine ? "The door to port slides open. " : "")}";

                if (context.CurrentLocation is BlatherLocation)
                    action += "Blather, looking slightly disoriented, barks at you to resume your assigned duties. ";
                break;

            case DeckNine.TurnWhenFeinsteinBlowsUp + 1:
            {
                action = context.CurrentLocation switch
                {
                    _ when context.CurrentLocation is BlatherLocation =>
                        "You are deafened by more explosions and by the sound of emergency bulkheads slamming closed. Blather, foaming slightly at the mouth, screams at you to swab the decks.",
                    _ when context.CurrentLocation is DeckNine =>
                        "\n\nMore distant explosions! A narrow emergency bulkhead at the base of the gangway and a wider one along the corridor to starboard both crash shut! ",
                    _ when context.CurrentLocation is Gangway =>
                        "Another explosion. A narrow bulkhead at the base of the gangway slams shut! ",
                    _ =>
                        "\n\nThe ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing. "
                };

                break;
            }

            case DeckNine.TurnWhenFeinsteinBlowsUp + 2:
            {
                if (context.CurrentLocation is not DeckNine && context.CurrentLocation is not EscapePod)
                {
                    var result =
                        "\n\nThe ship rocks from the force of multiple explosions. The lights go out, and you feel a " +
                        "sudden drop in pressure accompanied by a loud hissing. Too bad you weren't in the escape pod...";

                    action = YouExploded(context, result);
                    break;
                }

                Repository.GetItem<BulkheadDoor>().IsOpen = false;
                action =
                    $"\n\nMore powerful explosions buffet the ship. The lights flicker madly" +
                    $"{(context.CurrentLocation is DeckNine ? ", and the escape-pod bulkhead clangs shut" : "")}. ";
                break;
            }

            case DeckNine.TurnWhenFeinsteinBlowsUp + 3:
                action = "\n\nExplosions continue to rock the ship. ";
                break;

            case DeckNine.TurnWhenFeinsteinBlowsUp + 4:
            {
                var result =
                    "\n\nAn enormous explosion tears the walls of the ship apart. If only you had made it to an escape pod...";

                action = YouExploded(context, result);
                break;
            }
        }

        return action;
    }
}