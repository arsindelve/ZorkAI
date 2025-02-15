using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class PileOfPlastic : ContainerBase, ICanBeTakenAndDropped, ISubLocation, ICanBeExamined
{
    [UsedImplicitly] public bool IsPunctured { get; set; }

    public bool IsInflated { get; set; }

    public override bool IsTransparent => true;

    public override string[] NounsForMatching =>
        ["pile", "pile of plastic", "plastic", "boat", "punctured boat", "raft", "magic boat"];

    public override int Size => 14;

    public string ExaminationDescription => IsPunctured
        ? "There's nothing special about the punctured boat. "
        : IsInflated
            ? "There's nothing special about the magic boat. "
            : "There's nothing special about the pile of plastic. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return
            IsPunctured
                ? "There is a punctured boat here. "
                : IsInflated
                    ? "There is a magic boat here. " +
                      (Items.Any() ? ItemListDescription("magic boat", currentLocation) : "")
                    : "There is a folded pile of plastic here which has a small valve attached. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    // TODO: Items on the ground outside the boat: There is a shovel here. (outside the magic boat)

    string ISubLocation.GetIn(IContext context)
    {
        if (IsPunctured)
            return "You have a theory on how to board a punctured boat, perhaps? ";

        if (!IsInflated)
            return "You have a theory on how to board a pile of plastic, perhaps? ";

        if (context.GetItems<IAmPointyAndPunctureThings>().Any())
        {
            IsPunctured = true;
            IsInflated = false;
            return
                "Oops! Something sharp seems to have slipped and punctured the boat. The boat deflates to the " +
                "sounds of hissing, sputtering, and cursing. ";
        }

        context.CurrentLocation.SubLocation = this;
        return "You are now in the magic boat. ";
    }

    string ISubLocation.GetOut(IContext context)
    {
        if (context.CurrentLocation is IFrigidRiver)
            return "You realize that getting out here would be fatal. ";

        context.CurrentLocation.SubLocation = null;
        return "You are on your own feet again. ";
    }

    public string LocationDescription => ", in the magic boat";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return IsPunctured
            ? "A punctured boat"
            : IsInflated
                ? "A magic boat" + (Items.Any()
                    ? Environment.NewLine + ItemListDescription("magic boat", currentLocation)
                    : "")
                : "A pile of plastic";
    }

    public override void Init()
    {
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["inflate", "blow up", "blow"], NounsForMatching))
        {
            if (context.HasItem<PileOfPlastic>())
                return new PositiveInteractionResult("The boat must be on the ground to be inflated. ");

            return new PositiveInteractionResult("You don't have enough lung power to inflate it. ");
        }

        // TODO: Deflate
        // if (action.Match(["deflate"], NounsForMatching))
        // {
        //     if (context.HasItem<PileOfPlastic>())
        //         return new PositiveInteractionResult("The boat must be on the ground to be inflated. ");
        //
        //     return new PositiveInteractionResult("You don't have enough lung power to inflate it. ");
        // }

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        var leakNouns = NounsForMatching.Concat(["leak", "puncture", "hole"]).ToArray();

        var fixIntentMatch = action.Match<ViscousMaterial>(Verbs.FixVerbs, leakNouns,
            ["with", "using"]);

        fixIntentMatch |= action.Match(Verbs.ApplyVerbs, Repository.GetItem<ViscousMaterial>().NounsForMatching,
            leakNouns,
            ["on", "to", "against", "in"]);

        if (fixIntentMatch)
            return FixTheLeak(context);

        string[] verbs = ["inflate", "blow", "blow up", "use"];
        string[] prepositions = ["with", "to", "on", "using"];

        if (!action.MatchNounOne(NounsForMatching))
            return base.RespondToMultiNounInteraction(action, context);

        return InflateTheBoat(action, context, verbs, prepositions);
    }

    private InteractionResult InflateTheBoat(MultiNounIntent action, IContext context, string[] verbs,
        string[] prepositions)
    {
        if (context.HasItem<PileOfPlastic>())
            return new PositiveInteractionResult("The boat must be on the ground to be inflated. ");

        if (!action.MatchNounTwo<AirPump>())
            return base.RespondToMultiNounInteraction(action, context);

        if (!verbs.Contains(action.Verb.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        if (!prepositions.Contains(action.Preposition.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        if (!context.HasItem<AirPump>())
            return new PositiveInteractionResult("You don't have the air pump. ");

        if (IsInflated)
            return new PositiveInteractionResult("Inflating it further would probably burst it. ");

        if (IsPunctured)
            // This is REALLY, truly what the original game says. 
            return new PositiveInteractionResult("This boat will not inflate since some moron punctured it. ");

        IsInflated = true;
        ItemPlacedHere(Repository.GetItem<Label>());
        return new PositiveInteractionResult(
            "The boat inflates and appears seaworthy. A tan label is lying inside the boat. ");
    }

    private InteractionResult FixTheLeak(IContext context)
    {
        if (!IsPunctured)
            return new NoNounMatchInteractionResult();

        if (!context.HasItem<ViscousMaterial>())
            return new PositiveInteractionResult("You don't have the viscous material. ");

        IsPunctured = false;
        return new PositiveInteractionResult("Well done. The boat is repaired. ");
    }
}