using Model.AIGeneration;

namespace Planetfall.Item.Lawanda.Lab;

public class LabDesk : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["large desk", "messy desk", "desk"];

    [UsedImplicitly] public bool HasBeenSearched { get; set; }

    public string ExaminationDescription => GetExaminationDescription();

    private string GetExaminationDescription()
    {
        if (!HasBeenSearched)
        {
            HasBeenSearched = true;
            // The memo will be manually given to the player through game logic
            // For now, just return the text

            var deskState = IsOpen ? "open" : "closed, but it doesn't look locked";
            return $"After inspecting the various papers on the desk, you find only one item of interest, a memo of some sort. The desk itself is {deskState}. ";
        }

        return $"The desk has a drawer which is currently {(IsOpen ? "open" : "closed")}. ";
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // Handle examination and give memo to player
        if (action.MatchVerb(["examine", "search", "look at"]) && !HasBeenSearched)
        {
            HasBeenSearched = true;
            var memo = Repository.GetItem<Memo>();
            context.Take(memo);

            var deskState = IsOpen ? "open" : "closed, but it doesn't look locked";
            return new PositiveInteractionResult(
                $"After inspecting the various papers on the desk, you find only one item of interest, a memo of some sort. The desk itself is {deskState}. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override void Init()
    {
        StartWithItemInside<GasMask>();
    }

    public override string NowOpen(ILocation currentLocation)
    {
        if (Items.Any())
        {
            // Make gas mask the referent for "it"
            return $"Opening the desk reveals {SingleLineListOfItems()}. ";
        }
        return "Opened. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        if (IsOpen && Items.Any())
            return $"\n{ItemListDescription("desk", null)}";

        return base.GenericDescription(currentLocation);
    }
}
