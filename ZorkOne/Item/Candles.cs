namespace ZorkOne.Item;

public class Candles : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, IAmALightSource
{
    public bool Lit { get; set; }

    public override string[] NounsForMatching => ["candle", "candles"];

    public string ExaminationDescription => Lit ? "The candles are burning. " : "The candles are out. ";

    public string OnTheGroundDescription => "There is a pair of candles here " + (Lit ? "(providing light). " : ".");

    public override string NeverPickedUpDescription => "On the two ends of the altar are burning candles. ";
}