using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Utilities;

namespace Planetfall.Item.Lawanda.Library.Computer;

public class ComputerTerminal : ItemBase, ICanBeExamined, ICanBeRead, ITurnOffAndOn
{
    [UsedImplicitly] public MenuState MenuState { get; set; } = new();

    public override string[] NounsForMatching =>
        ["terminal", "computer terminal", "computer", "screen", "keyboard", "keypad", "keys"];

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
        // Handle phrasings where the player names both the key and the terminal/keyboard as
        // separate nouns, e.g. "type 1 on the keyboard" or "key 4 on the terminal". One noun
        // must refer to this terminal; the other is the number to key in.
        if (!action.MatchVerb(Verbs.TypeVerbs.Union(["press", "push"]).ToArray()))
            return await base.RespondToMultiNounInteraction(action, context);

        var nounOneIsTerminal = action.MatchNounOne(NounsForMatching);
        var nounTwoIsTerminal = action.MatchNounTwo(NounsForMatching);

        if (!nounOneIsTerminal && !nounTwoIsTerminal)
            return await base.RespondToMultiNounInteraction(action, context);

        // The key to press is whichever noun isn't the terminal itself.
        var keyNoun = nounOneIsTerminal ? action.NounTwo : action.NounOne;
        return ProcessKeyPress(keyNoun.ToInteger(), context);
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(Verbs.TypeVerbs.Union(["press", "push"]).ToArray()))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        return ProcessKeyPress(action.Noun.ToInteger(), context);
    }

    /// <summary>
    /// Applies a single numeric keypress to the menu, mirroring the original terminal: zero moves
    /// up a level, any other digit selects that submenu item. Floyd comments on the first use.
    /// </summary>
    private InteractionResult ProcessKeyPress(int? keyPress, IContext context)
    {
        // A dark screen takes no input: gate keypresses on power so the menu can't be blind-navigated
        // (and Floyd can't comment on "first use") while the terminal is off — matching how
        // ExaminationDescription and ReadDescription already report "The screen is dark." when off.
        if (!IsOn)
            return new PositiveInteractionResult("Nothing happens; the screen is dark. ");

        if (!keyPress.HasValue)
            return new PositiveInteractionResult("The keyboard only has the keys 0 through 9");

        Repository.GetItem<Floyd>().CommentOnAction(FloydPrompts.LibraryComputerFirstUse, context);

        if (keyPress.Value == 0)
            return new PositiveInteractionResult(MenuState.GoUp());

        return new PositiveInteractionResult(MenuState.GoDown(keyPress.Value));
    }
}