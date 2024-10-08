﻿using GameEngine.Item;
using Model.AIGeneration;
using Model.Interface;
using ZorkOne.Command;

namespace ZorkOne.Item;

public class Cyclops : ItemBase, ICanBeExamined, ITurnBasedActor
{
    public override string Name => "Cyclops";

    public bool HasBeenAttacked { get; set; }
    
    public bool HasGoneToSleep { get; set; }
    
    public bool HasBeenFed { get; set; }

    public int TurnsSinceAttacked { get; set; }
    
    // >give bottle to cyclops
    // The cyclops apparently is not thirsty and refuses your generous offer.
    
    //The cyclops says "Mmm Mmm. I love hot peppers! But oh, could I use a drink. Perhaps I could drink the blood of that thing."  From
    //    the gleam in his eye, it could be surmised that you are "that thing".
    
    // >give garlic to cyclops
    // The cyclops may be hungry, but there is a limit.
    
    
    public override string NeverPickedUpDescription(ILocation currentLocation) => HasBeenAttacked
        ? "he cyclops is standing in the corner, eyeing you closely. I don't think he likes you very much. He " +
          "looks extremely hungry, even for a cyclops."
        : "A cyclops, who looks prepared to eat horses (much less mere adventurers), blocks the staircase. " +
          "From his state of health, and the bloodstains on the walls, you gather that he is not very friendly, " +
          "though he likes people. ";

    public override string GenericDescription(ILocation currentLocation) => NeverPickedUpDescription(currentLocation);

    public override string[] NounsForMatching => ["cyclops"];

    public override string CannotBeTakenDescription => "The cyclops doesn't take kindly to being grabbed. ";

    public string ExaminationDescription => "A hungry cyclops is standing at the foot of the stairs. ";
    
    // TODO: Turn on the actor when you attack him or feed him. 
    // TODO: Other solution with peppers and water. 
    // TODO: Glowing sword. 
    
    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!HasBeenAttacked && !HasBeenFed)
            return Task.FromResult(string.Empty);

        TurnsSinceAttacked++;

        switch (TurnsSinceAttacked)
        {
            case 1:
                return Task.FromResult<string>("The cyclops seems somewhat agitated. ");
            case 2:
                return Task.FromResult<string>("TThe cyclops appears to be getting more agitated. ");
            case 3:
                return Task.FromResult<string>("The cyclops is moving about the room, looking for something. ");
            case 4:
                return Task.FromResult<string>(
                    "The cyclops was looking for salt and pepper. I think he is gathering condiments for his upcoming snack. ");
            case 5:
                return Task.FromResult<string>("The cyclops is moving about the room, looking for something. ");
            case 6:
                return Task.FromResult<string>("The cyclops is moving toward you in an unfriendly manner. ");
            case 7:
                return Task.FromResult<string>("You have two choices: 1. Leave  2. Become dinner. ");
        }
        
        context.RemoveActor(this);

        return Task.FromResult(new DeathProcessor().Process(
            "The cyclops, tired of all of your games and trickery, grabs you firmly. As he licks his chops, he says \"Mmm. Just like Mom used to make 'em.\" It's nice to be appreciated. ",
            context).InteractionMessage);
    }
}