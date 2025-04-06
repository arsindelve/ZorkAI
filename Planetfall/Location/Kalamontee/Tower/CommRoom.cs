using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee.Tower;

internal class CommRoom : LocationWithNoStartingItems
{
    private const string FixedDescription =
        " A screen on the console displays a message. Next to the screen is a flashing sign which " +
        "says \"Tranzmishun in pragres.\" Next to this console is an enunciator whose lights are all dark. " +
        "On the console next to the enunciator panel is a funnel-shaped hole labelled \"Kuulint Sistum Manyuuwul Oovuriid.\"";

    public override string Name => "Comm Room";

    [UsedImplicitly] public bool SystemIsCritical { get; set; }

    [UsedImplicitly] public bool IsFixed { get; set; }

    [UsedImplicitly] public string? CurrentColor { get; set; } = "Black";

    private string BrokenDescription => "A screen on the console displays a message. Next to the screen " +
                                        "is a flashing sign which says \"Malfunkshun in Sendeeng Kuulint Sistum.\" Next to this console " +
                                        "is an enunciator. On the console next to the enunciator panel is a funnel-shaped hole " +
                                        "labelled \"Kuulint Sistum Manyuuwul Oovuriid.\"" +
                                        $"\n\nA {CurrentColor} colored light is flashing on the enunciator panel.";

    private string CriticalDescription =>
        "A screen on the console displays a message. Next to the screen is " +
        "a flashing sign which says \"Kuulint Sistum Imbalins Kritikul -- " +
        "Shuteeng Down Awl Sistumz.\" Next to this console is an enunciator " +
        "whose lights are all dark. ";


    public override Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        string[] liquidNouns = ["liquid", "coolant", "fluid", "flask", "chemical"];
        string[] holeNouns = ["hole", "funnel", "console", "machine", "equipment", "system"];
        string[] verbs = ["empty", "pour", "dump"];
        string[] prepositions = ["into", "down", "in"];

        if (!action.Match(verbs, liquidNouns, holeNouns, prepositions))
            return base.RespondToMultiNounInteraction(action, context);

        if (string.IsNullOrEmpty(GetItem<Flask>().LiquidColor))
            return base.RespondToMultiNounInteraction(action, context);

        return PourLiquid(context);
    }

    private Task<InteractionResult?> PourLiquid(IContext context)
    {
        var flaskColor = GetItem<Flask>().LiquidColor!;
        GetItem<Flask>().LiquidColor = null;

        if (flaskColor != CurrentColor)
            return PermanentlyBroken();

        if (CurrentColor == "Black")
            return NextColor();

        return Fixed(context);
    }

    private Task<InteractionResult?> NextColor()
    {
        CurrentColor = "Gray";
        return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(
            "The liquid disappears into the hole. The lights on the enunciator panel blink rapidly " +
            "and all go off except one, a gray light."));
    }

    private Task<InteractionResult?> Fixed(IContext context)
    {
        context.AddPoints(6);
        IsFixed = true;
        CurrentColor = null;
        GetLocation<SystemsMonitors>().MarkCommunicationsFixed();

        return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(
            "The liquid disappears into the hole. The lights on the enunciator panel blink rapidly and then " +
            "go dark. The coolant system warning light goes off, and another flashes, indicating that the help " +
            "message is now being sent. "));
    }

    private Task<InteractionResult?> PermanentlyBroken()
    {
        SystemIsCritical = true;
        return Task.FromResult<InteractionResult?>(
            new PositiveInteractionResult("An alarm sounds briefly, and a sign flashes " +
                                          "\"Kuulint Sistum Imbalins Kritikul -- Shuteeng Down Awl Sistumz.\" A" +
                                          " moment later, the lights in the room dim and the send console shuts down. "));
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.SW, Go<TowerCore>() }
        };
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.PushVerbs, ["button"]))
            return new PositiveInteractionResult(
                "A voice fills the room ... the voice of the Feinstein's communications officer! \"Stellar Patrol " +
                "Ship Feinstein to planetside ... Please respond on frequency 48.5 ... SPS Feinstein to planetside ... " +
                "Please come in ...\" After a pause you hear the officer, in a quieter voice, say \"Admiral, no response " +
                "on any of the standard frequen...\" The sentence is cut short by the sound of an explosion and a loud " +
                "burst of static, followed by silence.");

        if (action.Match(["read", "examine"], ["screen", "console", "message"]))
            return new PositiveInteractionResult(
                "\"Tuu enee ship uv xe Sekund Galaktik Yuunyun: Planitwiid plaag haz struk entiir popyuulaashun. " +
                "Tiim iz kritikul. Eemurjensee asistins reekwestid. <reepeet\nmesij>\"");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a small room with no windows. The sole exit is southwest. Two wide consoles fill either end of the room; " +
            "thick cables lead up into the ceiling.\n\nThe console on the left side of the room is labelled " +
            "\"Reeseev Staashun.\" A bright red light, labelled \"Tranzmishun Reeseevd\", is blinking rapidly. " +
            "Next to the light is a glowing button marked \"Mesij Plaabak.\"\n\nThe console on the right side of " +
            "the room is labelled \"Send Staashun.\" \n\n" + (IsFixed ? FixedDescription :
                SystemIsCritical ? CriticalDescription : BrokenDescription);
    }
}