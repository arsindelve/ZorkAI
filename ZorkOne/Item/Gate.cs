namespace ZorkOne.Item;

public class Gate : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching  => ["gate"];

    public string ExaminationDescription =>
        "The gate is protected by an invisible force. It makes your teeth ache to touch it.";
}