namespace Planetfall.Item.Lawanda.Library.Computer;

public class ComputerTerminal : ItemBase, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["terminal", "computer terminal", "computer"];

    public string ExaminationDescription =>
        "The computer terminal consists of a video display screen, a keyboard with ten keys numbered from zero through nine, and an on-off switch. ";
    public string ReadDescription { get; }
}