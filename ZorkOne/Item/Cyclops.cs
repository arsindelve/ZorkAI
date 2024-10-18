using GameEngine;
using GameEngine.IntentEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using ZorkOne.Command;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Item;

public class Cyclops : ItemBase, ICanBeExamined, ITurnBasedActor, ICanBeAttacked, ICanBeGivenThings
{
    private readonly GiveSomethingToSomeoneDecisionEngine<Cyclops> _giveHimSomethingEngine = new();
    public override string Name => "Cyclops";

    /// <summary>
    ///     The cyclops gets agitated when you 1) Attack him 2) Feed him the hot pepper lunch
    ///     (until you give him the water, and then he goes to sleep)
    /// </summary>
    public bool IsAgitated { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public bool IsSleeping { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public int TurnsSinceAgitated { get; set; }

    public override string[] NounsForMatching => ["cyclops", "giant", "monster", "beast"];

    public override string CannotBeTakenDescription => "The cyclops doesn't take kindly to being grabbed. ";

    public bool IsDead { get; set; }

    public string ExaminationDescription => IsSleeping
        ? "The cyclops is sleeping like a baby, albeit a very ugly one. "
        : "A hungry cyclops is standing at the foot of the stairs. ";

    InteractionResult ICanBeGivenThings.OfferThisThing(IItem item, IContext context)
    {
        return item switch
        {
            Bottle => OfferTheBottle(context),
            Lunch => OfferTheLunch(context),
            Garlic => new PositiveInteractionResult("The cyclops may be hungry, but there is a limit. "),
            _ => new PositiveInteractionResult("The cyclops is not so stupid as to eat THAT! ")
        };
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!IsAgitated)
            return Task.FromResult(string.Empty);

        TurnsSinceAgitated++;

        // if the adventurer is out of the room, the countdown continues,
        // you just don't get notified of it. You're still cyclops meat. 

        if (context.CurrentLocation is not CyclopsRoom)
            return Task.FromResult(string.Empty);

        switch (TurnsSinceAgitated)
        {
            case 1:
                return Task.FromResult<string>("\nThe cyclops seems somewhat agitated. ");
            case 2:
                return Task.FromResult<string>("\nThe cyclops appears to be getting more agitated. ");
            case 3:
                return Task.FromResult<string>("\nThe cyclops is moving about the room, looking for something. ");
            case 4:
                return Task.FromResult<string>(
                    "\nThe cyclops was looking for salt and pepper. I think he is gathering condiments for his upcoming snack. ");
            case 5:
                return Task.FromResult<string>("\nThe cyclops is moving toward you in an unfriendly manner. ");
            case 6:
                return Task.FromResult<string>("\nYou have two choices: 1. Leave  2. Become dinner. ");
        }

        context.RemoveActor(this);

        return Task.FromResult(new DeathProcessor().Process(
            "The cyclops, tired of all of your games and trickery, grabs you firmly. As he licks his chops, " +
            "he says \"Mmm. Just like Mom used to make 'em.\" It's nice to be appreciated. ",
            context).InteractionMessage);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        var result = _giveHimSomethingEngine.AreWeGivingSomethingToSomeone(action, this, context);

        if (result is not null) 
            return result;
        
        return base.RespondToMultiNounInteraction(action, context);
    }

    private InteractionResult OfferTheLunch(IContext context)
    {
        IsAgitated = true;
        var message =
            " The cyclops says \"Mmm Mmm. I love hot peppers! But oh, could I use a drink. Perhaps I could drink " +
            "the blood of that thing.\"  From the gleam in his eye, it could be surmised that you are \"that thing\".";

        var lunch = Repository.GetItem<Lunch>();
        context.RemoveItem(lunch);
        lunch.CurrentLocation = null;

        return new PositiveInteractionResult(message);
    }

    private InteractionResult OfferTheBottle(IContext context)
    {
        if (!IsAgitated)
            return new PositiveInteractionResult(
                "The cyclops apparently is not thirsty and refuses your generous offer. ");

        IsSleeping = true;
        IsAgitated = false;
        context.RemoveActor(this);

        var bottle = Repository.GetItem<Bottle>();
        var water = Repository.GetItem<Water>();

        water.CurrentLocation = null;
        bottle.RemoveItem(water);
        context.Drop(bottle);

        return new PositiveInteractionResult(
            "The cyclops takes the bottle and drinks the water. A moment later, he lets out a yawn " +
            "that nearly blows you over, and then falls fast asleep (what did you put in that drink, anyway?). ");
    }

    public override string NeverPickedUpDescription(ILocation? currentLocation)
    {
        return IsSleeping
            ? "The cyclops is sleeping blissfully at the foot of the stairs. "
            : IsAgitated
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