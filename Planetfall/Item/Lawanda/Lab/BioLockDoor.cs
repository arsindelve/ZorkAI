using Model.AIGeneration;

namespace Planetfall.Item.Lawanda.Lab;

internal class BioLockDoor : LabLockDoor, ITurnBasedActor
{
    public override string[] NounsForMatching =>
    [
        "bio-lock door", "door", "biolock", "bio lock", "bio lock door", "bio-lock", "bio door"
    ];

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        throw new NotImplementedException();
    }
}