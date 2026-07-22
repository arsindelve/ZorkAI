using GameEngine.Location;
using Model.AIGeneration;
using Newtonsoft.Json;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Location.Kalamontee.Admin;

public class AdminCorridorSouth : LocationBase, ITurnBasedActor
{
    [UsedImplicitly] public bool HasSeenTheLight { get; set; }

    [UsedImplicitly] public bool HasTakenTheKey { get; set; }

    // Use the injectable chooser, not Random directly, so the glint-of-light daemon is deterministic
    // under test (CLAUDE.md: "Never use `Random` directly in game code - always use `IRandomChooser`").
    [UsedImplicitly] [JsonIgnore]
    public IRandomChooser Chooser { get; set; } = new RandomChooser();

    public override string Name => "Admin Corridor South";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (HasSeenTheLight || HasTakenTheKey)
            return Task.FromResult(string.Empty);

        // One-in-three chance per turn, matching the original Random.Shared.Next(3) == 0.
        if (Chooser.RollDiceSuccess(3))
            return Task.FromResult(
                "\n\nYou catch, out of the corner of your eye, a glint of light from the direction of the floor. ");

        return Task.FromResult(string.Empty);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<CorridorJunction>() },
            { Direction.N, Go<AdminCorridor>() },
            { Direction.E, Go<SanfacE>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This section of hallway seems to have suffered some minor structural damage. The walls are cracked, and " +
            "a jagged crevice crosses the floor. An opening leads east and the corridor heads north and south. " +
            // Only show the hint while the key is still in the crevice; suppress once retrieved.
            (HasSeenTheLight && !HasTakenTheKey ? "Lying at the bottom of a narrow crevice is a shiny object. " : "");
    }

    public override void Init()
    {
        StartWithItem<Key>();
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        context.RegisterActor(this);
        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        var magnetNouns = Repository.GetItem<Magnet>().NounsForMatching;

        // The crevice and its neighbours. "hole" is one of the crevice's ZIL synonyms
        // (compone.zil: SYNONYM CREVICE CRACK HOLE).
        string[] crackNouns = ["ground", "crack", "crevice", "key", "floor", "hole"];

        // Framing B — the magnet is the subject, lowered toward or into the crack. The original
        // only accepted put/place/hold + on/over/beside/next to, so the natural "lower it INTO the
        // crack" phrasings fell through to the AI narrator. Accept the full range of natural
        // "place/lower the bar at the crack" verbs and prepositions (issue #298).
        var match = action.Match(
            ["put", "place", "hold", "lower", "dangle", "dip", "stick", "insert",
                "set", "lay", "rest", "slide", "slip", "feed", "thread", "hang", "push", "poke",
                "move", "wave", "swing"],
            magnetNouns, crackNouns,
            ["on", "over", "beside", "next to", "in", "into", "down", "down into",
                "to", "near", "by", "against", "inside", "toward", "towards"]);

        // "drop" only solves when the magnet is lowered INTO the crack ("drop magnet in crevice").
        // A bare "drop magnet on the floor" is a player setting the bar down, not fishing for the
        // key, so keep it off the on/over/beside list above.
        match |= action.Match(["drop"], magnetNouns, crackNouns, ["in", "into", "down", "down into"]);

        // "use" / "apply" the magnet on/in/against the crack or key.
        match |= action.Match(["use", "apply"], magnetNouns, crackNouns,
            ["on", "in", "into", "to", "against", "near"]);

        // Magnetic-attraction framing — aim/point/touch the magnet AT or TO the key in the crack.
        match |= action.Match(["aim", "point", "touch", "connect", "attach"], magnetNouns, crackNouns,
            ["at", "to", "toward", "towards", "against", "near"]);

        // Framing A — the magnet is the tool and the key is the object retrieved "with" it. This is
        // the original game's canonical solve (compone.zil KEY-F: TAKE/ZATTRACT/MOVE the key with
        // PRSI = MAGNET). The port never wired it up, so "get key with magnet" fell through to the
        // AI narrator, which improvised a refusal calling the steel key "non-magnetic" — directly
        // contradicting the success text. Match only the key's own nouns here, not the crack/floor
        // synonyms: you retrieve the key "with" the magnet, you don't "lift the floor with" it.
        var keyNouns = Repository.GetItem<Key>().NounsForMatching;
        // The take family comes from Verbs.TakeVerbs rather than a partial hand-rolled copy - the
        // old inline list omitted "snatch"/"acquire"/"hold" (issue #406's drift class), so those
        // phrasings fell to the narrator's contradictory "non-magnetic" improvisation.
        match |= action.Match(
            Verbs.TakeVerbs.Concat(
                ["retrieve", "lift", "fish", "pull", "attract",
                    "remove", "snag", "hook", "fetch", "extract", "scoop", "catch", "collect", "recover",
                    "yank", "drag", "reel", "pluck", "nab", "haul", "obtain", "draw"]).ToArray(),
            keyNouns, magnetNouns, ["with", "using", "to", "toward", "towards"]);

        // Only a magnet/key-fishing attempt reaches here. Anything else falls through to base so an
        // unrelated two-noun command (e.g. "put brush in uniform") is handled normally — the old code
        // gated the "you don't have the bar" hint on the magnet's mere presence in the room, so it
        // swallowed EVERY two-noun command while the magnet was set down here (issue #436, the
        // multi-noun sibling of the examine catch-all fixed in #291).
        if (!match)
            return await base.RespondToMultiNounInteraction(action, context);

        // It's a genuine fishing attempt, but the player isn't holding the magnet.
        if (!context.HasItem<Magnet>())
            return Repository.GetItem<Magnet>().CurrentLocation == this
                ? new PositiveInteractionResult("You don't have the curved metal bar. ")
                : new NoNounMatchInteractionResult();

        if (HasTakenTheKey)
            return new PositiveInteractionResult("Nothing interesting happens. ");

        HasTakenTheKey = true;
        context.ItemPlacedHere<Key>();

        Repository.GetItem<Floyd>().CommentOnAction(FloydPrompts.MagnetRetrievesKey, context);

        return new PositiveInteractionResult(
            "With a spray of dust and a loud clank, a piece of metal leaps from the crevice and " +
            "affixes itself to the magnet. It is a steel key! With a tug, you remove the key from the magnet. ");
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(Verbs.ExamineVerbs))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        if (action.MatchNoun(["floor", "ground"]))
            return new PositiveInteractionResult("A narrow, jagged crevice crosses the floor. ");

        if (action.MatchNoun(["crevice", "crack", "hole", "light"]))
        {
            if (HasTakenTheKey)
                return new PositiveInteractionResult("A narrow, jagged crevice crosses the floor. ");

            HasSeenTheLight = true;
            return new PositiveInteractionResult(
                "Lying at the bottom of the narrow crack, partly covered by layers of dust, is a shiny steel key! ");
        }

        // Key is in the room's Items list from Init(), so base would find it and expose it early.
        // Guard all of Key's synonyms (key/steel key/shiny object/shiny thing) until discovered.
        if (!HasSeenTheLight && !HasTakenTheKey &&
            action.MatchNounAndAdjective(Repository.GetItem<Key>().NounsForMatching))
            return new PositiveInteractionResult("You don't see any key here. ");

        // Not a crevice-specific noun — let normal item/examine routing handle it.
        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
