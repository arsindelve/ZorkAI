using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Spirits : ItemBase, ICanBeExamined, ITurnBasedActor
{
    public bool Stunned { get; set; }

    public int StunnedCounter { get; set; }

    public override string[] NounsForMatching => ["spirits", "ghosts", "wraiths"];

    public override string CannotBeTakenDescription => ExaminationDescription;

    public string ExaminationDescription => "You seem unable to interact with these spirits. ";

    public string? Act(IContext context, IGenerationClient client)
    {
        if (!Stunned)
        {
            context.RemoveActor(this);
            return "";
        }

        StunnedCounter--;

        if (StunnedCounter == 0)
        {
            Stunned = false;
            var hades = Repository.GetLocation<EntranceToHades>();
            if (context.CurrentLocation == hades && Repository.GetItem<Spirits>().CurrentLocation == hades)
                return
                    "\nThe tension of this ceremony is broken, and the wraiths, amused but shaken at your clumsy attempt, resume their hideous jeering.";
        }

        return "";
    }

    public string BecomeStunned(IContext context)
    {
        context.RegisterActor(this);
        Stunned = true;
        StunnedCounter = 5;

        return
            "The wraiths, as if paralyzed, stop their jeering and slowly turn to face you. " +
            "On their ashen faces, the expression of a long-forgotten terror takes shape. ";
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.MatchNoun(NounsForMatching))
            return new PositiveInteractionResult(ExaminationDescription);

        return base.RespondToSimpleInteraction(action, context, client);
    }
}