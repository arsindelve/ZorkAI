namespace Planetfall.Item.Feinstein;

internal class PatrolUniformPocket : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["pocket", "uniform pocket", "patrol uniform pocket"];

    public override bool IsOpen => true;

    protected override int SpaceForItems => 1;

    public override bool IsTransparent => true;

    public override string AlreadyOpen => "There's no way to open or close the pocket of the Patrol uniform. ";
    
    public string ExaminationDescription => ItemListDescription("Patrol uniform pocket", null);

    public override void Init()
    {
        StartWithItemInside<IdCard>();
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        // Don't add extra indentation - the parent uniform already indents
        return string.Join("\n", Items.Select(s => s.GenericDescription(currentLocation)));
    }

    public override string? CannotBeClosedDescription(IContext context)
    {
        return "There's no way to open or close the pocket of the Patrol uniform. ";
    }
}