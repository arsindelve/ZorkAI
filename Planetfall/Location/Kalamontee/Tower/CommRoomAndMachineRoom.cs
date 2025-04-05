using GameEngine.Location;
using Model.AIGeneration;

namespace Planetfall.Location.Kalamontee.Tower;

internal class CommRoomAndMachineRoom : LocationWithNoStartingItems
{
    private const string FixedDescription =
        " A screen on the console displays a message. Next to the screen is a flashing sign which " +
        "says \"Tranzmishun in pragres.\" Next to this console is an enunciator whose lights are all dark. " +
        "On the console next to the enunciator panel is a funnel-shaped hole labelled \"Kuulint Sistum Manyuuwul Oovuriid.\"";

    public override string Name => "Comm Room";

    [UsedImplicitly] public bool IsFixed { get; set; }

    [UsedImplicitly] public string? CurrentColor { get; set; } = "Black";

    private string BrokenDescription => "A screen on the console displays a message. Next to the screen " +
                                        "is a flashing sign which says \"Malfunkshun in Sendeeng Kuulint Sistum.\" Next to this console " +
                                        "is an enunciator. On the console next to the enunciator panel is a funnel-shaped hole " +
                                        "labelled \"Kuulint Sistum Manyuuwul Oovuriid.\"" +
                                        $"\n\nA {CurrentColor} colored light is flashing on the enunciator panel.";

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
        if (action.Match(["press", "push", "activate"], ["button"]))
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
            "the room is labelled \"Send Staashun.\" " + (IsFixed ? FixedDescription : BrokenDescription);
    }
}