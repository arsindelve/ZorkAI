namespace Stationfall.Item.Duffy;

/// <summary>
///     The uniform's single pocket. It holds the ID card and cannot be opened or closed — the original
///     rebuffs any attempt (ship.zil:143-145).
/// </summary>
internal class PatrolUniformPocket : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["pocket", "uniform pocket", "patrol uniform pocket"];

    public override bool IsOpen => true;

    protected override int SpaceForItems => 10;

    public override bool IsTransparent => true;

    public override string AlreadyOpen => "There's no way to open or close the pocket of the Patrol uniform. ";

    public string ExaminationDescription => ItemListDescription("Patrol uniform pocket", null);

    public override void Init()
    {
        StartWithItemInside<IdCard>();
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        // The parent uniform already indents, so don't add another level here.
        return string.Join("\n", Items.Select(s => s.GenericDescription(currentLocation)));
    }
}
