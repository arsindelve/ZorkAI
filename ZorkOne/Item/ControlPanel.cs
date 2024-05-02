using Model.AIGeneration;
using Model.Intent;

namespace ZorkOne.Item;

public class ControlPanel : ItemBase, ICanBeTakenAndDropped
{
    public bool GreenBubbleGlowing { get; set; }

    public override string[] NounsForMatching => ["panel", "control", "control panel"];

    public override string CannotBeTakenDescription => "You can't be serious.";

    public override string Name => "control panel";

    public string OnTheGroundDescription => NeverPickedUpDescription;

    public override string NeverPickedUpDescription =>
        "There is a control panel here, on which a large metal bolt is mounted. " +
        $"Directly above the bolt is a small green plastic bubble{(GreenBubbleGlowing ? " which is glowing serenely" : "")}.";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        string[] matchingNouns = ["bolt", "bubble", "green bubble", "small bubble", "small green bubble"];

        var verb = action.Verb.ToLowerInvariant().Trim();
        var noun = action.Noun?.ToLowerInvariant().ToLowerInvariant().Trim();

        if (!matchingNouns.Contains(noun))
            return base.RespondToSimpleInteraction(action, context, client);

        return verb switch
        {
            "take" => new PositiveInteractionResult("It is an integral part of the control panel. "),
            "examine" => new PositiveInteractionResult($"There's nothing special about the {noun}. "),
            _ => base.RespondToSimpleInteraction(action, context, client)
        };
    }
}

// https://www.retrogamedeconstructionzone.com/2020/06/zork-i-commentary-and-puzzle-breakdown.html

// I initially misinterpreted the function of the yellow and brown buttons, so the dam stumped me for a while.
// Pushing them doesn't lead to any immediately obvious effects, they just respond with a "click".  To see their
// effects you need to go back out to the dam controls and notice that the "small green plastic bubble" begins to
// glow when you push yellow (any number of times), and stops glowing when you push brown.
// 
// It all seems very simple in retrospect, but making this connection took a while.  There is a bolt on the dam
// controls, and it will only turn if the green plastic bubble is glowing.  I can't tell you how many combinations
// of items I tried to use on this bolt before realizing the connection with the green plastic bubble.  Once
// the green plastic bubble is glowing, you can turn the bolt with the wrench. 