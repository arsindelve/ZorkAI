namespace Stationfall.Item.Duffy;

/// <summary>
///     A form slot. Three rooms aboard the Duffy have one, and each accepts exactly one form
///     (globals.zil:703-764, FORM-SLOT-F).
///     Each room gets its <em>own</em> slot type rather than sharing one instance: a single item seeded
///     into several rooms has its CurrentLocation overwritten by whichever room initializes last, and
///     scope checks then reject it everywhere else.
/// </summary>
public abstract class FormSlotBase : ContainerBase, ICanBeExamined
{
    private static readonly string[] InsertVerbs =
        ["put", "insert", "place", "slide", "feed", "push", "stick", "drop"];

    private static readonly string[] Prepositions = ["in", "into", "in to", "inside", "through"];

    public override string[] NounsForMatching => ["slot", "form slot"];

    // Nothing ever rests inside a slot: it swallows a form or spits it back.
    protected override int SpaceForItems => 0;

    public virtual string ExaminationDescription =>
        "A narrow slot, exactly wide enough to swallow a standard Patrol form. ";

    /// <summary>
    ///     The form this particular slot is willing to accept, or null if it accepts none.
    /// </summary>
    protected abstract Type? AcceptedFormType { get; }

    /// <summary>
    ///     Consume the correct form and report what the machinery says.
    /// </summary>
    protected abstract string AcceptForm(FormBase form, IContext context);

    /// <summary>
    ///     Refusal for the right kind of object at the wrong slot, or for a form this slot won't take.
    /// </summary>
    protected virtual string RejectForm(FormBase form, IContext context)
    {
        return "The slot spits the form back out. A recorded voice says, \"Improper form for this " +
               "location.\" ";
    }

    public override void Init()
    {
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        if (!IsInsertIntoThisSlot(action))
            return await base.RespondToMultiNounInteraction(action, context);

        var form = ResolveForm(action);

        if (form is null)
            return await base.RespondToMultiNounInteraction(action, context);

        if (AcceptedFormType is null || form.GetType() != AcceptedFormType)
            return new PositiveInteractionResult(RejectForm(form, context));

        if (form.Consumed)
            return new PositiveInteractionResult("The slot has already taken that form. ");

        if (!context.Items.Contains(form))
            return new PositiveInteractionResult("You aren't holding it. ");

        return new PositiveInteractionResult(AcceptForm(form, context));
    }

    private bool IsInsertIntoThisSlot(MultiNounIntent action)
    {
        return action.MatchVerb(InsertVerbs)
               && action.MatchNounTwo(NounsForMatching)
               && (action.MatchPreposition(Prepositions) || string.IsNullOrWhiteSpace(action.Preposition));
    }

    /// <summary>
    ///     Works out which form the player named. A bare "form" is taken to mean the one this slot
    ///     wants, which is what the player almost always intends.
    /// </summary>
    private FormBase? ResolveForm(MultiNounIntent action)
    {
        if (action.ItemOne is FormBase resolved)
            return resolved;

        FormBase[] forms =
        [
            Repository.GetItem<RobotUseAuthorizationForm>(),
            Repository.GetItem<ClassThreeSpacecraftActivationForm>(),
            Repository.GetItem<AssignmentCompletionForm>()
        ];

        var named = forms.FirstOrDefault(form => form.NounsForMatching
            .Any(noun => noun.Equals(action.NounOne, StringComparison.InvariantCultureIgnoreCase)
                         && !noun.Equals("form", StringComparison.InvariantCultureIgnoreCase)));

        if (named is not null)
            return named;

        var isGenericFormNoun = action.NounOne.Equals("form", StringComparison.InvariantCultureIgnoreCase)
                                || action.NounOne.Equals("forms", StringComparison.InvariantCultureIgnoreCase);

        if (!isGenericFormNoun)
            return null;

        return AcceptedFormType is null
            ? Repository.GetItem<AssignmentCompletionForm>()
            : forms.FirstOrDefault(form => form.GetType() == AcceptedFormType);
    }
}
