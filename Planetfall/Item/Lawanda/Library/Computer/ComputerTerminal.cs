using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Utilities;

namespace Planetfall.Item.Lawanda.Library.Computer;

public class ComputerTerminal : ItemBase, ICanBeExamined, ICanBeRead, ITurnOffAndOn
{
    [UsedImplicitly] public MenuState MenuState { get; set; } = new();

    public override string[] NounsForMatching => ["terminal", "computer terminal", "computer", "screen"];

    public string ExaminationDescription =>
        "The computer terminal consists of a video display screen, a keyboard with ten keys numbered from zero through nine, and an on-off switch. " +
        (IsOn ? $"The screen displays some writing:\n{MenuState.CurrentItem.Text}" : "The screen is dark. ");

    public string ReadDescription => IsOn ? MenuState.CurrentItem.Text ?? "" : "The screen is dark. ";

    public bool IsOn { get; set; }

    public string NowOnText => "The screen gives off a green flash, and then some writing appears on the screen: \n" +
                               MenuState.CurrentItem.Text;

    public string NowOffText => "The screen goes dark. ";

    public string AlreadyOffText => "It isn't on. ";

    public string AlreadyOnText => "It's already on. ";

    public string? CannotBeTurnedOnText => null;

    public string OnBeingTurnedOn(IContext context)
    {
        return string.Empty;
    }

    public void OnBeingTurnedOff(IContext context)
    {
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "On the table is a computer terminal. ";
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        // TODO: type 1 on the keyboard
        // TODO: key in 4 on the terminal. 
        return await base.RespondToMultiNounInteraction(action, context);
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(Verbs.TypeVerbs.Union(["press", "push"]).ToArray()))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        var keyPress = action.Noun.ToInteger();

        if (keyPress.HasValue)
        {
            Repository.GetItem<Floyd>().CommentOnAction(FloydPrompts.LibraryComputerFirstUse, context);

            if (keyPress.Value == 0)
                return new PositiveInteractionResult(MenuState.GoUp());

            return new PositiveInteractionResult(MenuState.GoDown(keyPress.Value));
        }

        return new PositiveInteractionResult("The keyboard only has the keys 0 through 9");
    }
}