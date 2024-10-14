using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using ZorkOne.Command;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Item;

public class Cyclops : ItemBase, ICanBeExamined, ITurnBasedActor, ICanBeAttacked
{
    public override string Name => "Cyclops";

    /// <summary>
    ///     The cyclops gets agitated when you 1) Attack him 2) Feed him the hot pepper lunch
    ///     (until you give him the water, and then he goes to sleep)
    /// </summary>
    public bool IsAgitated { get; set; }

    public bool HasGoneToSleep { get; set; }

    public int TurnsSinceAgitated { get; set; }

    public override string[] NounsForMatching => ["cyclops"];

    public override string CannotBeTakenDescription => "The cyclops doesn't take kindly to being grabbed. ";

    public bool IsDead { get; set; }

    public string ExaminationDescription => "A hungry cyclops is standing at the foot of the stairs. ";

    // TODO: Turn on the actor when you attack him or feed him. 
    // TODO: Other solution with peppers and water. 
    // TODO: Glowing sword. 

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!IsAgitated)
            return Task.FromResult(string.Empty);

        TurnsSinceAgitated++;

        // if the adventurer is out of the room, the countdown continues,
        // you just don't get notified of it.

        if (context.CurrentLocation is not CyclopsRoom)
            return Task.FromResult(string.Empty);

        switch (TurnsSinceAgitated)
        {
            case 1:
                return Task.FromResult<string>("The cyclops seems somewhat agitated. ");
            case 2:
                return Task.FromResult<string>("The cyclops appears to be getting more agitated. ");
            case 3:
                return Task.FromResult<string>("The cyclops is moving about the room, looking for something. ");
            case 4:
                return Task.FromResult<string>(
                    "The cyclops was looking for salt and pepper. I think he is gathering condiments for his upcoming snack. ");
            case 5:
                return Task.FromResult<string>("The cyclops is moving toward you in an unfriendly manner. ");
            case 6:
                return Task.FromResult<string>("You have two choices: 1. Leave  2. Become dinner. ");
        }

        context.RemoveActor(this);

        return Task.FromResult(new DeathProcessor().Process(
            "The cyclops, tired of all of your games and trickery, grabs you firmly. As he licks his chops, " +
            "he says \"Mmm. Just like Mom used to make 'em.\" It's nice to be appreciated. ",
            context).InteractionMessage);
    }

    // >give bottle to cyclops
    // The cyclops apparently is not thirsty and refuses your generous offer.

    //The cyclops says "Mmm Mmm. I love hot peppers! But oh, could I use a drink. Perhaps I could drink the blood of that thing."  From
    //    the gleam in his eye, it could be surmised that you are "that thing".

    // >give garlic to cyclops
    // The cyclops may be hungry, but there is a limit.

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        if (!action.MatchVerb(Verbs.GiveVerbs))
            return base.RespondToMultiNounInteraction(action, context);

        if (action.Preposition != "to")
            return base.RespondToMultiNounInteraction(action, context);
        
        return base.RespondToMultiNounInteraction(action, context);

        // Cyclops can be either noun...."give thing to cyclops" or "give cyclops the thing"
    }

    public override string NeverPickedUpDescription(ILocation? currentLocation)
    {
        return IsAgitated
            ? "he cyclops is standing in the corner, eyeing you closely. I don't think he likes you very much. He " +
              "looks extremely hungry, even for a cyclops. "
            : "A cyclops, who looks prepared to eat horses (much less mere adventurers), blocks the staircase. " +
              "From his state of health, and the bloodstains on the walls, you gather that he is not very friendly, " +
              "though he likes people. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return NeverPickedUpDescription(currentLocation);
    }
}