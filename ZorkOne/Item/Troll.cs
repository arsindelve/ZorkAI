using Model.AIGeneration;
using Model.Interface;
using ZorkOne.ActorInteraction;

namespace ZorkOne.Item;

public class Troll : ContainerBase, ICanBeExamined, ITurnBasedActor
{
    private readonly TrollCombatEngine _trollAttackEngine = new();

    public bool IsUnconscious { get; set; }

    public bool IsStunned { get; set; }

    public bool IsDead { get; set; }

    public override string[] NounsForMatching => ["troll"];

    public override string NeverPickedUpDescription => ExaminationDescription;

    public override string InInventoryDescription => ExaminationDescription;

    public override string CannotBeTakenDescription => IsUnconscious
        ? ""
        : "The troll spits in your face, grunting \"Better luck next time\" in a rather barbarous accent. ";

    public string ExaminationDescription => IsUnconscious
        ? "An unconscious troll is sprawled on the floor. All passages out of the room are open. "
        : !HasItem<BloodyAxe>()
            ? ""
            : "A nasty-looking troll, brandishing a bloody axe, blocks all passages out of the room. ";

    public string? Act(IContext context, IGenerationClient client)
    {
        if (IsDead || IsUnconscious)
            return null;

        if (!HasItem<BloodyAxe>())
            return
                "\nThe troll, disarmed, cowers in terror, pleading for his life in the guttural tongue of the trolls. ";

        return _trollAttackEngine.Attack(context);
    }

    // TODO: Give the axe back to the troll: The troll scratches his head in confusion, then takes the axe.
    // TODO: Give other stuff to the troll: The troll, who is not overly proud, graciously accepts the gift and, being for the moment sated, throws it back. Fortunately, the troll has poor control, and the sword falls to the floor. He does not look pleased.
    // TODO: >give knife to troll The troll, who is not overly proud, graciously accepts the gift and eats it hungrily. Poor troll, he dies from an internal hemorrhage and his carcass disappears in a sinister black fog. Your sword is no longer glowing.

    public override void Init()
    {
        StartWithItemInside<BloodyAxe>();
    }
}

// https://github.com/PDP-10/zork/blob/134871aea9cab2cf91982efad038091d416dddf8/dung.mud#L904
// https://github.com/PDP-10/zork/blob/134871aea9cab2cf91982efad038091d416dddf8/dung.mud#L1119